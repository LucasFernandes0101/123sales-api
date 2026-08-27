using _123vendas.Application.Services;
using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Unit.Mocks.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;
using Xunit;

namespace _123vendas.Unit.Services;

public class ProductServiceTest
{
    [Fact(DisplayName = "CreateAsync_Should_Create_Product_Successfully")]
    [Trait("Product", "Service")]
    public async Task CreateAsync_Should_Create_Product_Successfully()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();
        validator.ValidateAsync(mockProduct).Returns(Task.FromResult(new ValidationResult()));
        productRepository.AddAsync(mockProduct).Returns(mockProduct);

        // Act
        var result = await productService.CreateAsync(mockProduct);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(mockProduct);
        await productRepository.Received(1).AddAsync(mockProduct);
    }

    [Fact(DisplayName = "CreateAsync_Should_Throw_ValidationException_When_Validation_Fails")]
    [Trait("Product", "Service")]
    public async Task CreateAsync_Should_Throw_ValidationException_When_Validation_Fails()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();

        validator.ValidateAsync(mockProduct).Throws(new ValidationException("Validation failed."));

        // Act & Assert
        var act = async () => await productService.CreateAsync(mockProduct);
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Validation failed.");
    }

    [Fact(DisplayName = "DeleteAsync_Should_Delete_Product_Successfully")]
    [Trait("Product", "Service")]
    public async Task DeleteAsync_Should_Delete_Product_Successfully()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();
        productRepository.GetByIdAsync(mockProduct.Id).Returns(mockProduct);

        // Act
        await productService.DeleteAsync(mockProduct.Id);

        // Assert
        await productRepository.Received(1).DeleteAsync(mockProduct);
    }

    [Fact(DisplayName = "DeleteAsync_Should_Throw_NotFoundException_When_Product_Not_Found")]
    [Trait("Product", "Service")]
    public async Task DeleteAsync_Should_Throw_NotFoundException_When_Product_Not_Found()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var invalidId = 999;
        productRepository.GetByIdAsync(invalidId).Returns(default(Product));

        // Act & Assert
        var act = async () => await productService.DeleteAsync(invalidId);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "GetAllAsync_Should_Return_Product_List")]
    [Trait("Product", "Service")]
    public async Task GetAllAsync_Should_Return_Product_List()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProducts = new List<Product> { new ProductMock().Generate(), new ProductMock().Generate() };
        productRepository.GetAsync(1, 10, Arg.Any<Expression<Func<Product, bool>>>())
            .Returns(new PagedResult<Product>(mockProducts.Count, mockProducts));

        // Act
        var result = await productService.GetAllAsync(null, null, null, null, null, null, null, null, 1, 10);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
    }

    [Fact(DisplayName = "UpdateAsync_Should_Update_Product_Successfully")]
    [Trait("Product", "Service")]
    public async Task UpdateAsync_Should_Update_Product_Successfully()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();
        productRepository.GetByIdAsync(mockProduct.Id).Returns(mockProduct);
        validator.ValidateAsync(mockProduct).Returns(Task.FromResult(new ValidationResult()));

        // Act
        var result = await productService.UpdateAsync(mockProduct.Id, mockProduct);

        // Assert
        result.Should().Be(mockProduct);
        await productRepository.Received(1).UpdateAsync(mockProduct);
    }

    [Fact(DisplayName = "UpdateAsync_Should_Throw_NotFoundException_When_Product_Not_Found")]
    [Trait("Product", "Service")]
    public async Task UpdateAsync_Should_Throw_NotFoundException_When_Product_Not_Found()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();
        productRepository.GetByIdAsync(mockProduct.Id).Returns(default(Product));

        // Act & Assert
        var act = async () => await productService.UpdateAsync(mockProduct.Id, mockProduct);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "UpdateAsync_Should_Update_BranchProduct_When_Name_Changes")]
    [Trait("Product", "Service")]
    public async Task UpdateAsync_Should_Update_BranchProduct_When_Name_Changes()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();
        mockProduct.Title = "Amstel";

        var updatedProduct = new Product
        {
            Id = mockProduct.Id,
            Title = "Heineken",
            Description = mockProduct.Description,
            Category = mockProduct.Category,
            Price = mockProduct.Price,
            IsActive = mockProduct.IsActive
        };

        productRepository.GetByIdAsync(mockProduct.Id).Returns(mockProduct);
        validator.ValidateAsync(Arg.Any<Product>()).Returns(Task.FromResult(new ValidationResult()));

        // Act
        await productService.UpdateAsync(mockProduct.Id, updatedProduct);

        // Assert
        await branchProductRepository.Received(1)
            .UpdateByProductIdAsync(mockProduct.Id, updatedProduct.Title, mockProduct.Category);
    }

    [Fact(DisplayName = "UpdateAsync_Should_Update_BranchProduct_When_Category_Changes")]
    [Trait("Product", "Service")]
    public async Task UpdateAsync_Should_Update_BranchProduct_When_Category_Changes()
    {
        // Arrange
        var (productRepository, branchProductRepository, validator, productService) = CreateDependencies();
        var mockProduct = new ProductMock().Generate();
        mockProduct.Category = ProductCategory.Juice;

        var updatedProduct = new Product
        {
            Id = mockProduct.Id,
            Title = mockProduct.Title,
            Description = mockProduct.Description,
            Category = ProductCategory.Beer,
            Price = mockProduct.Price,
            IsActive = mockProduct.IsActive
        };

        productRepository.GetByIdAsync(mockProduct.Id).Returns(mockProduct);
        validator.ValidateAsync(Arg.Any<Product>()).Returns(Task.FromResult(new ValidationResult()));

        // Act
        await productService.UpdateAsync(mockProduct.Id, updatedProduct);

        // Assert
        await branchProductRepository.Received(1)
            .UpdateByProductIdAsync(mockProduct.Id, mockProduct.Title!, updatedProduct.Category);
    }

    private (IProductRepository productRepository, IBranchProductRepository branchProductRepository, IValidator<Product> validator, ProductService productService) CreateDependencies()
    {
        var productRepository = Substitute.For<IProductRepository>();
        var branchProductRepository = Substitute.For<IBranchProductRepository>();
        var validator = Substitute.For<IValidator<Product>>();
        var productService = new ProductService(productRepository, branchProductRepository, validator);

        return (productRepository, branchProductRepository, validator, productService);
    }

}
