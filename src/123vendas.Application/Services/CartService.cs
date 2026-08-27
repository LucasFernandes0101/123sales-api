using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Domain.Interfaces.Services;
using FluentValidation;
using System.Linq.Expressions;

namespace _123vendas.Application.Services;

public class CartService(
    ICartRepository repository,
    IValidator<Cart> validator) : ICartService
{
    public async Task<Cart> CreateAsync(Cart request, CancellationToken cancellationToken = default)
    {
        try
        {
            await ValidateCartAsync(request, cancellationToken);

            return await repository.AddAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            throw new ServiceException("An error occurred while creating a cart.", ex);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await FindCartOrThrowAsync(id, cancellationToken);

            await repository.DeleteAsync(cart, cancellationToken);
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while deleting the cart.", ex);
        }
    }

    public async Task<PagedResult<Cart>> GetAllAsync(
        int? id = default,
        int? userId = default,
        DateTimeOffset? minDate = default,
        DateTimeOffset? maxDate = default,
        int page = 1,
        int maxResults = 10,
        string? orderByClause = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page <= 0 || maxResults <= 0)
                throw new InvalidPaginationParametersException("Page number and max results must be greater than zero.");

            var criteria = BuildCriteria(id, userId, minDate, maxDate);

            var result = await repository.GetAsync(page, maxResults, criteria, orderByClause, cancellationToken);

            return result;
        }
        catch (BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving cartes.", ex);
        }
    }

    public async Task<Cart?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await repository.GetWithProductsByIdAsync(id, cancellationToken);

            return cart;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while retrieving the cart.", ex);
        }
    }

    public async Task<Cart> UpdateAsync(int id, Cart request, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await UpdateCartAsync(id, request, cancellationToken);

            await ValidateCartAsync(cart, cancellationToken);

            return await repository.UpdateAsync(cart, cancellationToken);
        }
        catch (Exception ex) when (ex is ValidationException || ex is BaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ServiceException("An error occurred while updating the cart.", ex);
        }
    }

    private async Task<Cart> UpdateCartAsync(int id, Cart request, CancellationToken cancellationToken)
    {
        var existingCart = await FindCartWithProductsOrThrowAsync(id, cancellationToken);

        UpdateCartProperties(existingCart, request);

        UpdateAndRemoveProducts(existingCart, request.Products);

        return existingCart;
    }

    private static void UpdateAndRemoveProducts(Cart existingCart, List<CartProduct>? updatedProducts)
    {
        updatedProducts ??= [];

        foreach (var updatedProduct in updatedProducts)
        {
            var existingProduct = existingCart.Products?.Find(cp => 
                                    cp.ProductId == updatedProduct.ProductId);

            if (existingProduct is not null)
            {
                existingProduct.Quantity = updatedProduct.Quantity;
                continue;
            }

            AddNewProductToCart(existingCart, updatedProduct);
        }

        RemoveProductsNotInUpdatedList(existingCart, updatedProducts);
    }

    private static void AddNewProductToCart(Cart existingCart, CartProduct updatedProduct)
    {
        existingCart.Products ??= [];

        updatedProduct.CartId = existingCart.Id;
        existingCart.Products.Add(updatedProduct);
    }

    private static void RemoveProductsNotInUpdatedList(Cart existingCart, List<CartProduct> updatedProducts)
    {
        if (existingCart.Products is null) return;

        var productsToRemove = existingCart.Products
            .Where(cp => updatedProducts.Count(up => up.ProductId == cp.ProductId) == 0)
            .ToList();

        foreach (var product in productsToRemove)
            existingCart.Products.Remove(product);
    }

    private static void UpdateCartProperties(Cart existingCart, Cart request)
    {
        existingCart.UserId = request.UserId;
        existingCart.Date = request.Date;
    }

    private static Expression<Func<Cart, bool>> BuildCriteria(
        int? id,
        int? userId,
        DateTimeOffset? minDate,
        DateTimeOffset? maxDate)
        => b =>
            (!id.HasValue || b.Id == id.Value) &&
            (!userId.HasValue || b.UserId == userId) &&
            (!minDate.HasValue || b.Date >= minDate.Value) &&
            (!maxDate.HasValue || b.Date <= maxDate.Value);

    private async Task<Cart> FindCartOrThrowAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Cart with ID {id} not found.");

    private async Task<Cart> FindCartWithProductsOrThrowAsync(int id, CancellationToken cancellationToken)
        => await repository.GetWithProductsByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Cart with ID {id} not found.");

    private async Task ValidateCartAsync(Cart cart, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(cart, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
    }
}