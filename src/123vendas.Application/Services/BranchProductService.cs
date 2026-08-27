using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Domain.Interfaces.Services;
using FluentValidation;
using System.Linq.Expressions;

namespace _123vendas.Application.Services;

public class BranchProductService(
    IBranchProductRepository repository,
    IProductRepository productRepository,
    IValidator<BranchProduct> validator) : IBranchProductService
{
    public async Task<BranchProduct> CreateAsync(BranchProduct request, CancellationToken cancellationToken = default)
    {
        await ValidateBranchProductAsync(request, cancellationToken);

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {request.ProductId} not found.");

        MapProductDetailsToBranchProduct(request, product);

        return await repository.AddAsync(request, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var branchProduct = await FindBranchProductOrThrowAsync(id, cancellationToken);
            await repository.DeleteAsync(branchProduct, cancellationToken);
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while deleting the branch product.", ex);
        }
    }

    public async Task<PagedResult<BranchProduct>> GetAllAsync(
        int? id = default,
        int? branchId = default,
        int? productId = default,
        bool? isActive = default,
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

            var criteria = BuildCriteria(id, branchId, productId, isActive, startDate, endDate);

            var result = await repository.GetAsync(page, maxResults, criteria, orderByClause, cancellationToken);

            return result;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving branch products.", ex);
        }
    }

    public async Task<BranchProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var branchProduct = await repository.GetByIdAsync(id, cancellationToken);

            return branchProduct;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the branch product.", ex);
        }
    }

    public async Task<BranchProduct> UpdateAsync(int id, BranchProduct request, CancellationToken cancellationToken = default)
    {
        var branchProduct = await UpdateBranchProductAsync(id, request, cancellationToken);

        await ValidateBranchProductAsync(branchProduct, cancellationToken);

        return await repository.UpdateAsync(branchProduct, cancellationToken);
    }

    private async Task ValidateBranchProductAsync(BranchProduct request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
    }

    private async Task<BranchProduct> UpdateBranchProductAsync(int id, BranchProduct request, CancellationToken cancellationToken)
    {
        var existingBranchProduct = await FindBranchProductOrThrowAsync(id, cancellationToken);

        existingBranchProduct.Price = request.Price;
        existingBranchProduct.StockQuantity = request.StockQuantity;
        existingBranchProduct.IsActive = request.IsActive;

        return existingBranchProduct;
    }

    private static void MapProductDetailsToBranchProduct(BranchProduct branchProduct, Product product)
    {
        branchProduct.ProductTitle = product.Title;
        branchProduct.ProductCategory = product.Category;
    }

    private static Expression<Func<BranchProduct, bool>> BuildCriteria(
        int? id,
        int? branchId,
        int? productId,
        bool? isActive,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
        => b =>
            (!id.HasValue || b.Id == id.Value) &&
            (!branchId.HasValue || b.BranchId == branchId.Value) &&
            (!productId.HasValue || b.ProductId == productId.Value) &&
            (!isActive.HasValue || b.IsActive == isActive.Value) &&
            (!startDate.HasValue || b.CreatedAt >= startDate.Value) &&
            (!endDate.HasValue || b.CreatedAt <= endDate.Value);

    private async Task<BranchProduct> FindBranchProductOrThrowAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"BranchProduct with ID {id} not found.");
}