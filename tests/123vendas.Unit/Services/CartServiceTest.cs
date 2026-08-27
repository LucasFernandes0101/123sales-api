using _123vendas.Application.Services;
using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Unit.Mocks.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace _123vendas.Unit.Services;

public class CartServiceTest
{
    [Fact(DisplayName = "CreateAsync should create a cart successfully")]
    [Trait("Cart", "Service")]
    public async Task CreateAsync_ShouldCreateCart_WhenValidInput()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();
        var cart = new CartMock().Generate();

        validator.ValidateAsync(cart).Returns(Task.FromResult(new ValidationResult()));
        repository.AddAsync(cart).Returns(cart);

        // Act
        var result = await service.CreateAsync(cart);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cart);
        await repository.Received(1).AddAsync(cart);
    }

    [Fact(DisplayName = "Should throw ValidationException when cart is invalid")]
    [Trait("Cart", "Service")]
    public async Task CreateAsync_ShouldThrowValidationException_WhenInvalid()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();
        var cart = new CartMock().Generate();

        var validationErrors = new List<ValidationFailure> { new("Date", "Date cannot be in the future.") };
        validator.ValidateAsync(cart).Returns(Task.FromResult(new ValidationResult(validationErrors)));

        // Act
        var act = () => service.CreateAsync(cart);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<Cart>());
    }

    [Fact(DisplayName = "Should delete cart successfully")]
    [Trait("Cart", "Service")]
    public async Task DeleteAsync_ShouldDeleteCart_WhenValidId()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var cart = new CartMock().Generate();
        repository.GetByIdAsync(cart.Id).Returns(cart);

        // Act
        await service.DeleteAsync(cart.Id);

        // Assert
        await repository.Received(1).DeleteAsync(cart);
    }

    [Fact(DisplayName = "Should throw NotFoundException when cart not found on delete")]
    [Trait("Cart", "Service")]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenCartNotFound()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var invalidId = 999;
        repository.GetByIdAsync(invalidId).Returns(default(Cart));

        // Act
        var act = () => service.DeleteAsync(invalidId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await repository.DidNotReceive().DeleteAsync(Arg.Any<Cart>());
    }

    [Fact(DisplayName = "Should retrieve all carts successfully")]
    [Trait("Cart", "Service")]
    public async Task GetAllAsync_ShouldReturnCarts_WhenValidParameters()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var carts = new CartMock().Generate(2);
        repository.GetAsync(1, 10, Arg.Any<Expression<Func<Cart, bool>>>())
            .Returns(Task.FromResult(new PagedResult<Cart>(carts.Count, carts)));

        // Act
        var result = await service.GetAllAsync(null, null, null, null, 1, 10, null);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().BeEquivalentTo(carts);
    }

    [Fact(DisplayName = "Should retrieve Cart by Id successfully")]
    [Trait("Cart", "Service")]
    public async Task GetByIdAsync_ShouldReturnCart_WhenCartExists()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var cart = new CartMock().Generate();
        repository.GetWithProductsByIdAsync(cart.Id).Returns(cart);

        // Act
        var result = await service.GetByIdAsync(cart.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cart);
    }

    [Fact(DisplayName = "Should throw ServiceException when an error occurs retrieving cart by Id")]
    [Trait("Cart", "Service")]
    public async Task GetByIdAsync_ShouldThrowServiceException_WhenErrorOccurs()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var invalidId = 999;
        repository.GetWithProductsByIdAsync(invalidId).Returns(Task.FromException<Cart?>(new Exception("Database error")));

        // Act
        var act = () => service.GetByIdAsync(invalidId);

        // Assert
        await act.Should().ThrowAsync<ServiceException>();
    }

    [Fact(DisplayName = "Should update cart successfully")]
    [Trait("Cart", "Service")]
    public async Task UpdateAsync_ShouldUpdateCart_WhenValidInput()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();
        var existingCart = new CartMock().Generate();
        var updatedCart = new CartMock().Generate();

        repository.GetWithProductsByIdAsync(existingCart.Id).Returns(existingCart);
        validator.ValidateAsync(existingCart).Returns(Task.FromResult(new ValidationResult()));
        repository.UpdateAsync(existingCart).Returns(Task.FromResult(existingCart));

        // Act
        var result = await service.UpdateAsync(existingCart.Id, updatedCart);

        // Assert
        result.Should().BeEquivalentTo(existingCart);
        await repository.Received(1).UpdateAsync(existingCart);
    }

    [Fact(DisplayName = "Should update quantity of existing products in the cart")]
    [Trait("Cart", "Service")]
    public async Task UpdateAsync_ShouldUpdateQuantity_WhenProductsExist()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var existingCart = new CartMock().Generate();

        var product = new CartProduct
        {
            Id = 1,
            Quantity = 5
        };

        existingCart.Products = [product];

        var updatedCart = new CartMock().Generate();

        updatedCart.Products = [product];
        updatedCart.Products!.First().Quantity = 10;

        repository.GetWithProductsByIdAsync(existingCart.Id).Returns(existingCart);
        validator.ValidateAsync(existingCart).Returns(Task.FromResult(new ValidationResult()));
        repository.UpdateAsync(existingCart).Returns(Task.FromResult(existingCart));

        // Act
        var result = await service.UpdateAsync(existingCart.Id, updatedCart);

        // Assert
        result.Should().BeEquivalentTo(existingCart);
        result.Products?.Find(p => p.Quantity == 10)?.Quantity.Should().Be(10);
        await repository.Received(1).UpdateAsync(existingCart);
    }

    [Fact(DisplayName = "Should throw NotFoundException when cart not found on update")]
    [Trait("Cart", "Service")]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenCartNotFound()
    {
        // Arrange
        var (repository, validator, service) = CreateDependencies();

        var cart = new CartMock().Generate();

        var invalidId = 999;
        repository.GetByIdAsync(invalidId).Returns(default(Cart));

        // Act
        var act = () => service.UpdateAsync(invalidId, cart);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await repository.DidNotReceive().UpdateAsync(Arg.Any<Cart>());
    }

    private static (ICartRepository repository, IValidator<Cart> validator, CartService service) CreateDependencies()
    {
        var repository = Substitute.For<ICartRepository>();
        var validator = Substitute.For<IValidator<Cart>>();
        var service = new CartService(repository, validator);

        return (repository, validator, service);
    }
}