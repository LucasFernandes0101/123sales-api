using _123vendas.Application.Events.Sales;
using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Integrations;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Domain.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace _123vendas.Application.Services;

public class SaleService : ISaleService
{
    private const int MAX_ITEMS_PER_SALE = 20;

    private readonly ISaleRepository _repository;
    private readonly ISaleItemRepository _saleItemRepository;
    private readonly IBranchProductRepository _branchProductRepository;
    private readonly IValidator<Sale> _validator;
    private readonly IRabbitMQIntegration _rabbitMQIntegration;
    private readonly ILogger<SaleService> _logger;

    public SaleService(ISaleRepository repository,
                       ISaleItemRepository saleItemRepository,
                       IBranchProductRepository branchProductRepository,
                       IValidator<Sale> validator,
                       IRabbitMQIntegration rabbitMQIntegration,
                       ILogger<SaleService> logger)
    {
        _repository = repository;
        _saleItemRepository = saleItemRepository;
        _branchProductRepository = branchProductRepository;
        _validator = validator;
        _rabbitMQIntegration = rabbitMQIntegration;
        _logger = logger;
    }

    public async Task<Sale> CreateAsync(Sale request)
    {
        try
        {
            var sale = new Sale
            {
                BranchId = request.BranchId,
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                Status = SaleStatus.Created,
                Items = new List<SaleItem>()
            };

            await ProcessItemsAsync(sale, request.Items!);

            await ValidateSaleAsync(sale);

            var savedSale = await _repository.AddAsync(sale);

            await UpdateStockQuantitiesAsync(savedSale.Items!, savedSale.BranchId);

            await PublishSaleMessageAsync(new SaleCreatedEvent(savedSale));

            return savedSale;
        }
        catch (Exception ex) when (ex is ValidationException || ex is BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while processing sale. Please try again later.", ex);
        }
    }

    public async Task<Sale> CancelItemAsync(int saleId, int sequence)
    {
        try
        {
            var sale = await GetSaleWithItemsOrThrowAsync(saleId);

            if (sale.Status == SaleStatus.Canceled)
                throw new SaleAlreadyCanceledException($"Cannot cancel an item from a sale that is already canceled.");

            var saleItem = sale?.Items?.FirstOrDefault(i => i.Sequence == sequence);

            if (saleItem is null)
                throw new NotFoundException($"Sale item sequence {sequence} not found.");

            ValidateItemForCancellation(saleItem);

            CancelItem(saleItem);

            sale!.TotalAmount = CalculateTotalAmount(sale.Items!);

            await _repository.UpdateAsync(sale);

            await PublishSaleMessageAsync(new SaleItemCancelledEvent(saleItem));

            return sale;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while canceling the sale item.", ex);
        }
    }

    public async Task<SaleItem> GetItemAsync(int saleId, int sequence)
    {
        try
        {
            var sale = await GetSaleWithItemsOrThrowAsync(saleId);

            var saleItem = sale?.Items?.FirstOrDefault(i => i.Sequence == sequence);

            if (saleItem is null)
                throw new NotFoundException($"Sale item sequence {sequence} not found in sale ID {saleId}.");

            return saleItem;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the item.", ex);
        }
    }

    public async Task<PagedResult<Sale>> GetAllAsync(int? id = default,
                                                     int? branchId = default,
                                                     int? userId = default,
                                                     SaleStatus? status = default,
                                                     DateTimeOffset? startDate = default,
                                                     DateTimeOffset? endDate = default,
                                                     int page = 1,
                                                     int maxResults = 10,
                                                     string? orderByClause = default)
    {
        try
        {
            if (page <= 0 || maxResults <= 0)
                throw new InvalidPaginationParametersException("Page number and max results must be greater than zero.");

            var criteria = BuildCriteria(id, branchId, userId, status, startDate, endDate);

            var result = await _repository.GetAsync(page, maxResults, criteria, orderByClause);

            return result;
        }
        catch (BaseException ex)
        {
            throw new ServiceException("An error occurred while retrieving sales.", ex);
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving sales.", ex);
        }
    }

    public async Task<Sale?> GetByIdAsync(int id)
    {
        try
        {
            var sale = await _repository.GetWithItemsByIdAsync(id);

            return sale;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the sale.", ex);
        }
    }

    public async Task<Sale> UpdateAsync(int saleId, Sale request)
    {
        try
        {
            var existingSale = await GetSaleWithItemsOrThrowAsync(saleId);

            ValidateForUpdate(existingSale);

            existingSale.Status = request.Status;
            existingSale.Date = request.Date;
            existingSale.UserId = request.UserId;
            existingSale.BranchId = request.BranchId;
            existingSale.CancelledAt = request.CancelledAt;

            if (request.Items is not null && request.Items.Count > 0)
            {
                existingSale.Items ??= new List<SaleItem>();
                existingSale.Items.Clear();

                await ProcessItemsAsync(existingSale, request.Items);
            }

            existingSale.TotalAmount = CalculateTotalAmount(existingSale.Items ?? new List<SaleItem>());

            await ValidateSaleAsync(existingSale);

            await _repository.UpdateAsync(existingSale);

            await PublishSaleMessageAsync(new SaleUpdatedEvent(existingSale));

            return existingSale;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while updating the sale.", ex);
        }
    }

    public async Task<Sale> CancelAsync(int saleId)
    {
        try
        {
            var sale = await _repository.GetByIdAsync(saleId);

            if (sale is null)
                throw new NotFoundException($"Sale with ID {saleId} not found.");

            if (sale.Status == SaleStatus.Canceled)
                throw new SaleAlreadyCanceledException($"This sale is already canceled.");

            sale.Status = SaleStatus.Canceled;
            sale.CancelledAt = DateTimeOffset.Now;

            await _repository.UpdateAsync(sale);

            await PublishSaleMessageAsync(new SaleCancelledEvent(sale));

            return sale;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while canceling the sale. Please try again later.", ex);
        }
    }

    public async Task DeleteAsync(int saleId)
    {
        try
        {
            var existingSale = await GetSaleOrThrowAsync(saleId);

            await _repository.DeleteAsync(existingSale);
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while deleting the sale. Please try again later.", ex);
        }
    }

    #region CreateSale

    private async Task ProcessItemsAsync(Sale sale, List<SaleItem> items)
    {
        short sequence = 1;

        foreach (var item in items)
        {
            var saleItem = await ProcessItemAsync(sale.BranchId, item, sequence);
            sale.Items!.Add(saleItem);
            sale.TotalAmount += saleItem.Price;

            sequence++;
        }
    }

    private async Task<SaleItem> ProcessItemAsync(int branchId, SaleItem requestItem, short sequence)
    {
        var branchProduct = await GetBranchProductOrThrowAsync(branchId, requestItem.ProductId);

        if (branchProduct.StockQuantity < requestItem.Quantity)
            throw new ItemOutOfStockException($"Product {branchProduct.ProductTitle} is out of stock.");

        if (requestItem.Quantity > MAX_ITEMS_PER_SALE)
            throw new ItemQuantityLimitExceededException("Cannot sell more than 20 identical items.");

        var saleItem = new SaleItem
        {
            ProductId = branchProduct.ProductId,
            ProductTitle = branchProduct.ProductTitle,
            UnitPrice = branchProduct.Price,
            Quantity = requestItem.Quantity,
            Sequence = sequence
        };

        saleItem.Discount = CalculateItemDiscount(requestItem);

        saleItem.Price = CalculateItemPrice(saleItem);

        return saleItem;
    }

    private decimal CalculateItemPrice(SaleItem item)
    {
        var discountMultiplier = 1 - (item.Discount / 100 ?? 0);
        return item.UnitPrice * item.Quantity * discountMultiplier;
    }

    private decimal CalculateItemDiscount(SaleItem item)
    {
        if (item.Quantity < 4)
            return 0;

        if (item.Discount.HasValue && item.Discount.Value > 0)
            return item.Discount.Value;

        if (item.Quantity >= 10)
            return 20;

        return 10;
    }

    private async Task UpdateStockQuantitiesAsync(List<SaleItem> items, int branchId)
    {
        foreach (var item in items)
        {
            var branchProduct = await GetBranchProductOrThrowAsync(branchId, item.ProductId);

            branchProduct.StockQuantity -= item.Quantity;
            await _branchProductRepository.UpdateAsync(branchProduct);
        }
    }

    #endregion

    private void ValidateForUpdate(Sale sale)
    {
        if (sale.Status == SaleStatus.Canceled)
            throw new SaleAlreadyCanceledException("Cannot update a canceled sale.");
    }

    private void ValidateItemForCancellation(SaleItem saleItem)
    {
        if (saleItem.IsCancelled)
            throw new SaleItemAlreadyCanceledException("This item is already cancelled.");
    }

    private void CancelItem(SaleItem saleItem)
    {
        saleItem.IsCancelled = true;
        saleItem.CancelledAt = DateTimeOffset.Now;
    }

    private decimal CalculateTotalAmount(List<SaleItem> items)
    {
        return items.Where(item => !item.IsCancelled).Sum(item => item.Price);
    }

    private async Task<Sale> GetSaleOrThrowAsync(int saleId)
    {
        var sale = await _repository.GetByIdAsync(saleId);
        if (sale is null)
            throw new NotFoundException($"Sale with ID {saleId} not found.");

        return sale;
    }

    private async Task<Sale> GetSaleWithItemsOrThrowAsync(int id)
    {
        var sale = await _repository.GetWithItemsByIdAsync(id);

        if (sale is null)
            throw new NotFoundException($"Sale with ID {id} not found.");

        return sale;
    }

    private async Task PublishSaleMessageAsync(BaseEvent @event)
    {
        try
        {
            await _rabbitMQIntegration.PublishMessageAsync(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while publishing event {@event.GetType().Name}");
        }
    }

    private async Task<BranchProduct> GetBranchProductOrThrowAsync(int branchId, int productId)
    {
        var branchProduct = (await _branchProductRepository.GetAsync(1, 1,
            p => p.IsActive && p.BranchId == branchId && p.ProductId == productId))
            .Items?.FirstOrDefault();

        if (branchProduct is null)
            throw new NotFoundException($"Product ID {productId} not found or inactive in branch ID {branchId}.");

        return branchProduct;
    }

    private async Task ValidateSaleAsync(Sale sale)
    {
        var validationResult = await _validator.ValidateAsync(sale);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
    }

    private Expression<Func<Sale, bool>> BuildCriteria(int? id,
                                                       int? branchId,
                                                       int? userId,
                                                       SaleStatus? status,
                                                       DateTimeOffset? startDate,
                                                       DateTimeOffset? endDate)
    {
        return b =>
            (!id.HasValue || b.Id == id.Value) &&
            (!branchId.HasValue || b.BranchId == branchId.Value) &&
            (!userId.HasValue || b.UserId == userId.Value) &&
            (!status.HasValue || b.Status == status.Value) &&
            (!startDate.HasValue || b.CreatedAt >= startDate.Value) &&
            (!endDate.HasValue || b.CreatedAt <= endDate.Value);
    }
}