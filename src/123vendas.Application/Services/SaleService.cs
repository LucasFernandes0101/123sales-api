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

public class SaleService(
    ISaleRepository repository,
    IBranchProductRepository branchProductRepository,
    IValidator<Sale> validator,
    IRabbitMQIntegration rabbitMQIntegration,
    ILogger<SaleService> logger) : ISaleService
{
    private const int MAX_ITEMS_PER_SALE = 20;

    public async Task<Sale> CreateAsync(Sale request, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = new Sale
            {
                BranchId = request.BranchId,
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                Status = SaleStatus.Created,
                Items = []
            };

            await ProcessItemsAsync(sale, request.Items!, cancellationToken);

            await ValidateSaleAsync(sale, cancellationToken);

            var savedSale = await repository.AddAsync(sale, cancellationToken);

            await UpdateStockQuantitiesAsync(savedSale.Items!, savedSale.BranchId, cancellationToken);

            await PublishSaleMessageAsync(new SaleCreatedEvent(savedSale), cancellationToken);

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

    public async Task<Sale> CancelItemAsync(int saleId, int sequence, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await GetSaleWithItemsOrThrowAsync(saleId, cancellationToken);

            if (sale.Status == SaleStatus.Canceled)
                throw new SaleAlreadyCanceledException($"Cannot cancel an item from a sale that is already canceled.");

            var saleItem = sale?.Items?.Find(item => 
                item.Sequence == sequence)
                ?? throw new NotFoundException($"Sale item sequence {sequence} not found.");

            ValidateItemForCancellation(saleItem);

            CancelItem(saleItem);

            sale!.TotalAmount = CalculateTotalAmount(sale.Items!);

            await repository.UpdateAsync(sale, cancellationToken);

            await PublishSaleMessageAsync(new SaleItemCancelledEvent(saleItem), cancellationToken);

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

    public async Task<SaleItem> GetItemAsync(int saleId, int sequence, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await GetSaleWithItemsOrThrowAsync(saleId, cancellationToken);

            var saleItem = sale?.Items?.Find(item => 
                item.Sequence == sequence)
                ?? throw new NotFoundException($"Sale item sequence {sequence} not found in sale ID {saleId}.");

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

    public async Task<PagedResult<Sale>> GetAllAsync(
        int? id = default,
        int? branchId = default,
        int? userId = default,
        SaleStatus? status = default,
        DateTimeOffset? startDate = default,
        DateTimeOffset? endDate = default,
        int page = 1,
        int maxResults = 10,
        string? orderByClause = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page <= 0 || maxResults <= 0)
                throw new InvalidPaginationParametersException("Page number and max results must be greater than zero.");

            var criteria = BuildCriteria(id, branchId, userId, status, startDate, endDate);

            var result = await repository.GetAsync(page, maxResults, criteria, orderByClause, cancellationToken);

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

    public async Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await repository.GetWithItemsByIdAsync(id, cancellationToken);

            return sale;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the sale.", ex);
        }
    }

    public async Task<Sale> UpdateAsync(int saleId, Sale request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingSale = await GetSaleWithItemsOrThrowAsync(saleId, cancellationToken);

            ValidateForUpdate(existingSale);

            existingSale.Status = request.Status;
            existingSale.Date = request.Date;
            existingSale.UserId = request.UserId;
            existingSale.BranchId = request.BranchId;
            existingSale.CancelledAt = request.CancelledAt;

            if (request.Items is not null && request.Items.Any())
            {
                existingSale.Items ??= [];
                existingSale.Items.Clear();

                await ProcessItemsAsync(existingSale, request.Items, cancellationToken);
            }

            existingSale.TotalAmount = CalculateTotalAmount(existingSale.Items ?? []);

            await ValidateSaleAsync(existingSale, cancellationToken);

            await repository.UpdateAsync(existingSale, cancellationToken);

            await PublishSaleMessageAsync(new SaleUpdatedEvent(existingSale), cancellationToken);

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

    public async Task<Sale> CancelAsync(int saleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await repository.GetByIdAsync(saleId, cancellationToken)
                ?? throw new NotFoundException($"Sale with ID {saleId} not found.");

            if (sale.Status == SaleStatus.Canceled)
                throw new SaleAlreadyCanceledException($"This sale is already canceled.");

            sale.Status = SaleStatus.Canceled;
            sale.CancelledAt = DateTimeOffset.Now;

            await repository.UpdateAsync(sale, cancellationToken);

            await PublishSaleMessageAsync(new SaleCancelledEvent(sale), cancellationToken);

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

    public async Task DeleteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingSale = await GetSaleOrThrowAsync(saleId, cancellationToken);

            await repository.DeleteAsync(existingSale, cancellationToken);
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

    private async Task ProcessItemsAsync(Sale sale, List<SaleItem> items, CancellationToken cancellationToken)
    {
        short sequence = 1;

        foreach (var item in items)
        {
            var saleItem = await ProcessItemAsync(sale.BranchId, item, sequence, cancellationToken);
            sale.Items!.Add(saleItem);
            sale.TotalAmount += saleItem.Price;

            sequence++;
        }
    }

    private async Task<SaleItem> ProcessItemAsync(int branchId, SaleItem requestItem, short sequence, CancellationToken cancellationToken)
    {
        var branchProduct = await GetBranchProductOrThrowAsync(branchId, requestItem.ProductId, cancellationToken);

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

    private static decimal CalculateItemPrice(SaleItem item)
    {
        var discountMultiplier = 1 - (item.Discount / 100 ?? 0);
        return item.UnitPrice * item.Quantity * discountMultiplier;
    }

    private static decimal CalculateItemDiscount(SaleItem item)
    {
        if (item.Quantity < 4)
            return 0;

        if (item.Discount.HasValue && item.Discount.Value > 0)
            return item.Discount.Value;

        if (item.Quantity >= 10)
            return 20;

        return 10;
    }

    private async Task UpdateStockQuantitiesAsync(List<SaleItem> items, int branchId, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var branchProduct = await GetBranchProductOrThrowAsync(branchId, item.ProductId, cancellationToken);

            branchProduct.StockQuantity -= item.Quantity;
            await branchProductRepository.UpdateAsync(branchProduct, cancellationToken);
        }
    }

    #endregion

    private static void ValidateForUpdate(Sale sale)
    {
        if (sale.Status == SaleStatus.Canceled)
            throw new SaleAlreadyCanceledException("Cannot update a canceled sale.");
    }

    private static void ValidateItemForCancellation(SaleItem saleItem)
    {
        if (saleItem.IsCancelled)
            throw new SaleItemAlreadyCanceledException("This item is already cancelled.");
    }

    private static void CancelItem(SaleItem saleItem)
    {
        saleItem.IsCancelled = true;
        saleItem.CancelledAt = DateTimeOffset.Now;
    }

    private static decimal CalculateTotalAmount(List<SaleItem> items)
        => items.Where(item => !item.IsCancelled).Sum(item => item.Price);

    private async Task<Sale> GetSaleOrThrowAsync(int saleId, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new NotFoundException($"Sale with ID {saleId} not found.");

    private async Task<Sale> GetSaleWithItemsOrThrowAsync(int id, CancellationToken cancellationToken)
        => await repository.GetWithItemsByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Sale with ID {id} not found.");

    private async Task PublishSaleMessageAsync(BaseEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            await rabbitMQIntegration.PublishMessageAsync(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while publishing event {EventType}", @event.GetType().Name);
        }
    }

    private async Task<BranchProduct> GetBranchProductOrThrowAsync(int branchId, int productId, CancellationToken cancellationToken)
    {
        var result = await branchProductRepository.GetAsync(1, 1,
            p => p.IsActive && p.BranchId == branchId && p.ProductId == productId, default, cancellationToken);

        return result.Items.Any()
            ? result.Items[0]
            : throw new NotFoundException($"Product ID {productId} not found or inactive in branch ID {branchId}.");
    }

    private async Task ValidateSaleAsync(Sale sale, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(sale, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
    }

    private static Expression<Func<Sale, bool>> BuildCriteria(
        int? id,
        int? branchId,
        int? userId,
        SaleStatus? status,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => b =>
            (!id.HasValue || b.Id == id.Value) &&
            (!branchId.HasValue || b.BranchId == branchId.Value) &&
            (!userId.HasValue || b.UserId == userId.Value) &&
            (!status.HasValue || b.Status == status.Value) &&
            (!startDate.HasValue || b.CreatedAt >= startDate.Value) &&
            (!endDate.HasValue || b.CreatedAt <= endDate.Value);
}