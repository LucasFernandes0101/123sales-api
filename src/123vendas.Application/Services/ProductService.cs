using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Domain.Interfaces.Services;
using FluentValidation;
using System.Linq.Expressions;

namespace _123vendas.Application.Services;

public class ProductService(
    IProductRepository repository,
    IBranchProductRepository branchProductRepository,
    IValidator<Product> validator) : IProductService
{
    public async Task<Product> CreateAsync(Product request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.Rating ??= new();

            await ValidateProductAsync(request, cancellationToken);

            return await repository.AddAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is ValidationException || ex is BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while creating a product.", ex);
        }
    }

    public IEnumerable<string> GetAllCategories()
        => Enum.GetValues<ProductCategory>()
            .Select(c => c.ToString());

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await FindProductOrThrowAsync(id, cancellationToken);

            await repository.DeleteAsync(product, cancellationToken);
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while deleting the product.", ex);
        }
    }

    public async Task<PagedResult<Product>> GetAllAsync(
        int? id = default,
        bool? isActive = default,
        string? title = default,
        string? category = default,
        decimal? minPrice = default,
        decimal? maxPrice = default,
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

            var criteria = BuildCriteria(id, isActive, title, category, minPrice, maxPrice, startDate, endDate);

            var result = await repository.GetAsync(page, maxResults, criteria, orderByClause, cancellationToken);

            return result;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving products.", ex);
        }
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await repository.GetByIdAsync(id, cancellationToken);

            return product;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the product.", ex);
        }
    }

    public async Task<Product> UpdateAsync(int id, Product request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingProduct = await FindProductOrThrowAsync(id, cancellationToken);

            var oldTitle = existingProduct.Title!;
            var oldCategory = existingProduct.Category;

            var product = await UpdateProductAsync(existingProduct, request);

            await ValidateProductAsync(product, cancellationToken);

            await repository.UpdateAsync(product, cancellationToken);

            if (!oldTitle.Equals(product.Title) || oldCategory != product.Category)
                await branchProductRepository.UpdateByProductIdAsync(product.Id, product.Title!, product.Category, cancellationToken);

            return product;
        }
        catch (Exception ex) when (ex is ValidationException || ex is BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while updating the product.", ex);
        }
    }

    private static Task<Product> UpdateProductAsync(Product existingProduct, Product request)
    {
        existingProduct.Title = request.Title;
        existingProduct.Description = request.Description;
        existingProduct.Image = request.Image;
        existingProduct.Category = request.Category;
        existingProduct.Price = request.Price;
        existingProduct.IsActive = request.IsActive;

        if(request.Rating is not null)
            existingProduct.Rating = request.Rating;

        return Task.FromResult(existingProduct);
    }

    private static Expression<Func<Product, bool>> BuildCriteria(
        int? id,
        bool? isActive,
        string? title,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate)
    {
        ProductCategory? categoryFilter = default;

        if (!string.IsNullOrWhiteSpace(category))
            if (Enum.TryParse<ProductCategory>(category, true, out var categoryEnum))
                categoryFilter = categoryEnum;

        return b =>
            (!id.HasValue || b.Id == id.Value) &&
            (!isActive.HasValue || b.IsActive == isActive.Value) &&
            (string.IsNullOrEmpty(title) ||
            (title.StartsWith('*') && title.EndsWith('*') ? b.Title!.Contains(title.Trim('*')) :
            title.StartsWith('*') ? b.Title!.EndsWith(title.TrimStart('*')) :
            title.EndsWith('*') ? b.Title!.StartsWith(title.TrimEnd('*')) :
            b.Title == title)) &&
            (!categoryFilter.HasValue || b.Category == categoryFilter.Value) &&
            (!minPrice.HasValue || b.Price >= minPrice.Value) &&
            (!maxPrice.HasValue || b.Price <= maxPrice.Value) &&
            (!startDate.HasValue || b.CreatedAt >= startDate.Value) &&
            (!endDate.HasValue || b.CreatedAt <= endDate.Value);
    }

    private async Task<Product> FindProductOrThrowAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {id} not found.");

    private async Task ValidateProductAsync(Product request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
    }
}