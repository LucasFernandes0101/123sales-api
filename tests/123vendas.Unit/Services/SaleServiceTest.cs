using _123vendas.Application.Events.Sales;
using _123vendas.Application.Services;
using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Integrations;
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

public class SaleServiceTest
{
    [Fact(DisplayName = "Create Sale - Valid Input")]
    [Trait("Sale", "Service")]
    public async Task CreateAsync_ShouldCreateSale_WhenValidRequest()
    {
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var saleMock = new SaleMock().Generate();
        saleMock.Status = SaleStatus.Created;
        saleMock.Items = [new SaleItem { ProductId = 1, Quantity = 1 }];

        var branchProduct = new BranchProduct { ProductId = saleMock.Items[0].ProductId, Price = 100, StockQuantity = 10, BranchId = saleMock.BranchId };

        repository.AddAsync(Arg.Any<Sale>()).Returns(saleMock);
        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(new PagedResult<BranchProduct>(1, [branchProduct]));
        validator.ValidateAsync(Arg.Any<Sale>()).Returns(new ValidationResult());

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var result = await saleService.CreateAsync(saleMock);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SaleStatus.Created);

        saleMock.Items.Count.Should().Be(1, "a sale should have exactly 1 item");
        saleMock.Items[0].Quantity.Should().Be(1, "the item quantity should be 1");

        await repository.Received(1).AddAsync(Arg.Is<Sale>(s => s.UserId == saleMock.UserId));
        await branchProductRepository.Received(1).UpdateAsync(Arg.Is<BranchProduct>(bp => bp.StockQuantity == 9));
        await rabbitMQIntegration.Received(1)
            .PublishMessageAsync(Arg.Is<SaleCreatedEvent>(e => e.Id == result.Id));
    }

    [Fact(DisplayName = "Create Sale - Validation Failed")]
    [Trait("Sale", "Service")]
    public async Task CreateAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();
        var saleMock = new SaleMock().Generate();
        var branchProducts = new BranchProductMock().Generate(1);

        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(Task.FromResult(new PagedResult<BranchProduct>(1, branchProducts)));
        validator.ValidateAsync(Arg.Any<Sale>()).Returns(new ValidationResult(new List<ValidationFailure> {
            new("UserId", "User is required") }));

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var act = async () => await saleService.CreateAsync(saleMock);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Create Sale - Product Out Of Stock")]
    [Trait("Sale", "Service")]
    public async Task CreateAsync_ShouldThrowItemOutOfStockException_WhenProductIsOutOfStock()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();
        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);
        var saleMock = new SaleMock().Generate();
        var branchProduct = new BranchProduct { ProductId = saleMock.Items!.First().ProductId, Price = 100, StockQuantity = 0 };

        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(new PagedResult<BranchProduct>(1, [branchProduct]));
        validator.ValidateAsync(saleMock).Returns(new ValidationResult());

        // Act
        var act = async () => await saleService.CreateAsync(saleMock);

        // Assert
        await act.Should().ThrowAsync<ItemOutOfStockException>();
    }

    [Fact(DisplayName = "Create Sale - BranchProduct Not Found")]
    [Trait("Sale", "Service")]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenBranchProductNotFound()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();
        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);
        var saleMock = new SaleMock().Generate();

        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(new PagedResult<BranchProduct>(0, []));
        validator.ValidateAsync(saleMock).Returns(new ValidationResult());

        // Act
        var act = async () => await saleService.CreateAsync(saleMock);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory(DisplayName = "Create Sale - Validate Discount on Items with Valid Discount Provided")]
    [Trait("Sale", "Service")]
    [InlineData(1)]
    [InlineData(4)]
    public async Task CreateSaleAsync_ShouldApplyDiscount_WhenValidDiscountProvided(int quantity)
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var sale = new SaleMock().Generate();
        sale.Items =
        [
            new SaleItem
            {
                ProductId = 1,
                Quantity = quantity,
                Discount = 25,
                Sequence = 2
            }
        ];

        var branchProduct = new BranchProduct
        {
            ProductId = sale.Items[0].ProductId,
            BranchId = 1,
            Price = 100,
            StockQuantity = 100,
        };

        validator.ValidateAsync(Arg.Any<Sale>()).Returns(new ValidationResult());

        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(Task.FromResult(new PagedResult<BranchProduct>(1, [branchProduct])));

        var capturedSale = new Sale();
        repository.AddAsync(Arg.Do<Sale>(s => capturedSale = s)).Returns(sale);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        await saleService.CreateAsync(sale);

        // Assert
        capturedSale.Should().NotBeNull();
        capturedSale.Items.Should().NotBeEmpty();

        if (quantity >= 4)
        {
            capturedSale.Items[0].Discount.Should().Be(25);
            capturedSale.Items[0].Price.Should().Be(300);
            capturedSale.TotalAmount.Should().Be(300);
        }
        else
        {
            capturedSale.Items[0].Discount.Should().Be(0);
            capturedSale.Items[0].Price.Should().Be(100);
            capturedSale.TotalAmount.Should().Be(100);
        }

        await repository.Received(1).AddAsync(Arg.Any<Sale>());
    }

    [Theory(DisplayName = "Create Sale - Apply Correct Discount Based on Item Quantity")]
    [Trait("Sale", "Service")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task CreateSaleAsync_ShouldApplyCorrectDiscount_BasedItemQuantity(int quantity)
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var sale = new SaleMock().Generate();
        sale.Items =
        [
            new SaleItem
            {
                ProductId = 1,
                Quantity = quantity,
                Sequence = 2
            }
        ];

        var branchProduct = new BranchProduct
        {
            ProductId = sale.Items[0].ProductId,
            BranchId = 1,
            Price = 100,
            StockQuantity = 100,
        };

        validator.ValidateAsync(Arg.Any<Sale>()).Returns(new ValidationResult());

        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(Task.FromResult(new PagedResult<BranchProduct>(1, [branchProduct])));

        var capturedSale = new Sale();
        repository.AddAsync(Arg.Do<Sale>(s => capturedSale = s)).Returns(sale);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        await saleService.CreateAsync(sale);

        // Assert
        capturedSale.Should().NotBeNull();
        capturedSale.Items.Should().NotBeEmpty();

        var expectedDiscount = 0m;
        var expectedPrice = branchProduct.Price * quantity;

        if (quantity >= 10)
        {
            expectedDiscount = 20;
            expectedPrice = branchProduct.Price * quantity * (1 - 0.20m);
        }
        else if (quantity >= 4)
        {
            expectedDiscount = 10;
            expectedPrice = branchProduct.Price * quantity * (1 - 0.10m);
        }

        capturedSale.Items[0].Discount.Should().Be(expectedDiscount);
        capturedSale.Items[0].Price.Should().Be(expectedPrice);
        capturedSale.TotalAmount.Should().Be(expectedPrice);

        await repository.Received(1).AddAsync(Arg.Any<Sale>());
    }

    [Fact(DisplayName = "Create Sale - Should Throw When Item Quantity More Than 20")]
    [Trait("Sale", "Service")]
    public async Task CreateSaleAsync_ShouldThrow_WhenItemQuantityMoreThan20()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var sale = new SaleMock().Generate();
        sale.Items =
        [
            new SaleItem
            {
                ProductId = 1,
                Quantity = 21,
                Discount = 25,
                Sequence = 2
            }
        ];

        var branchProduct = new BranchProduct
        {
            ProductId = sale.Items[0].ProductId,
            BranchId = 1,
            Price = 100,
            StockQuantity = 100,
        };

        validator.ValidateAsync(Arg.Any<Sale>()).Returns(new ValidationResult());

        branchProductRepository.GetAsync(1, 1, Arg.Any<Expression<Func<BranchProduct, bool>>>())
            .Returns(Task.FromResult(new PagedResult<BranchProduct>(1, [branchProduct])));

        var capturedSale = new Sale();
        repository.AddAsync(Arg.Do<Sale>(s => capturedSale = s)).Returns(sale);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var act = async () => await saleService.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ItemQuantityLimitExceededException>()
            .WithMessage("Cannot sell more than 20 identical items.");
    }

    [Fact(DisplayName = "Cancel Sale Item - Valid Request")]
    [Trait("Sale", "Service")]
    public async Task CancelItemAsync_ShouldCancelSaleItem_WhenValidRequest()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var sale = new SaleMock().Generate();
        sale.TotalAmount = sale.Items!.Sum(i => i.Price);
        sale.Status = SaleStatus.Created;
        var oldSaleAmount = sale.TotalAmount;

        repository.GetWithItemsByIdAsync(sale.Id).Returns(sale);
        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var result = await saleService.CancelItemAsync(sale.Id, sale.Items!.First().Sequence);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().Contain(i => i.IsCancelled);

        var canceledItemPrice = sale.Items!.First().Price;
        await repository.Received(1)
            .UpdateAsync(Arg.Is<Sale>(s => s.TotalAmount == (oldSaleAmount - canceledItemPrice)));

        await rabbitMQIntegration.Received(1)
            .PublishMessageAsync(Arg.Is<SaleItemCancelledEvent>(e => e.Sequence == sale.Items!.First().Sequence));
    }

    [Fact(DisplayName = "Cancel Sale Item - Sale Already Canceled")]
    [Trait("Sale", "Service")]
    public async Task CancelItemAsync_ShouldThrowException_WhenSaleAlreadyCanceled()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var sale = new SaleMock().Generate();
        sale.Status = SaleStatus.Canceled;

        repository.GetWithItemsByIdAsync(sale.Id).Returns(sale);
        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var act = () => saleService.CancelItemAsync(sale.Id, sale.Items!.First().Sequence);

        // Assert
        await act.Should().ThrowAsync<SaleAlreadyCanceledException>()
            .WithMessage("Cannot cancel an item from a sale that is already canceled.");
    }

    [Fact(DisplayName = "Cancel Sale Item - Item Already Canceled")]
    [Trait("Sale", "Service")]
    public async Task CancelItemAsync_ShouldThrowException_WhenItemAlreadyCanceled()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var sale = new SaleMock().Generate();
        sale.Status = SaleStatus.Created;
        sale.Items![0].IsCancelled = true;

        repository.GetWithItemsByIdAsync(sale.Id).Returns(sale);
        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var act = () => saleService.CancelItemAsync(sale.Id, sale.Items[0].Sequence);

        // Assert
        await act.Should().ThrowAsync<SaleItemAlreadyCanceledException>()
            .WithMessage("This item is already cancelled.");
    }

    [Fact(DisplayName = "Cancel Sale - Valid Request")]
    [Trait("Sale", "Service")]
    public async Task CancelAsync_ShouldCancelSale_WhenValidRequest()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);
        var saleMock = new SaleMock().Generate();
        saleMock.Status = SaleStatus.Created;
        repository.GetByIdAsync(saleMock.Id).Returns(saleMock);

        // Act
        var result = await saleService.CancelAsync(saleMock.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SaleStatus.Canceled);
        await repository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.CancelledAt.HasValue));
        await rabbitMQIntegration.Received(1)
            .PublishMessageAsync(Arg.Is<SaleCancelledEvent>(e => e.Id == result.Id));
    }

    [Fact(DisplayName = "Get All Sales - Valid Parameters")]
    [Trait("Sale", "Service")]
    public async Task GetAllAsync_ShouldReturnSales_WhenValidParameters()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);
        var sales = new SaleMock().Generate(1);
        repository.GetAsync(1, 10, Arg.Any<Expression<Func<Sale, bool>>>())
            .Returns(new PagedResult<Sale>(1, sales));

        // Act
        var result = await saleService.GetAllAsync(null, null, null, null, null, null);

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Should().BeEquivalentTo(sales.First());
    }

    [Fact(DisplayName = "Update Sale - Valid Request")]
    [Trait("Sale", "Service")]
    public async Task UpdateAsync_ShouldUpdateSale_WhenValidRequest()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var saleMock = new SaleMock().Generate();
        saleMock.Status = SaleStatus.Created;
        repository.GetWithItemsByIdAsync(saleMock.Id).Returns(saleMock);
        repository.UpdateAsync(Arg.Any<Sale>()).Returns(saleMock);
        validator.ValidateAsync(Arg.Is<Sale>(s => s.Id == saleMock.Id))
            .Returns(new ValidationResult());

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var result = await saleService.UpdateAsync(saleMock.Id, saleMock);

        // Assert
        result.Should().NotBeNull();
        await repository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.Id == saleMock.Id));
        await rabbitMQIntegration.Received(1)
            .PublishMessageAsync(Arg.Is<SaleUpdatedEvent>(e => e.Id == result.Id));
    }

    [Fact(DisplayName = "Delete Sale - Valid Request")]
    [Trait("Sale", "Service")]
    public async Task DeleteAsync_ShouldDeleteSale_WhenValidRequest()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var saleMock = new SaleMock().Generate();
        repository.GetByIdAsync(saleMock.Id).Returns(saleMock);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        await saleService.DeleteAsync(saleMock.Id);

        // Assert
        await repository.Received(1).DeleteAsync(Arg.Is<Sale>(s => s.Id == saleMock.Id));
    }

    [Fact(DisplayName = "Delete Sale - Sale Not Found")]
    [Trait("Sale", "Service")]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenSaleNotFound()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        repository.GetByIdAsync(Arg.Any<int>()).Returns((Sale?)null);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var act = () => saleService.DeleteAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Sale with ID 999 not found.");
    }

    [Fact(DisplayName = "Get Sale By Id - Valid Request")]
    [Trait("Sale", "Service")]
    public async Task GetByIdAsync_ShouldReturnSale_WhenValidRequest()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        var saleMock = new SaleMock().Generate();
        repository.GetWithItemsByIdAsync(saleMock.Id).Returns(saleMock);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var result = await saleService.GetByIdAsync(saleMock.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(saleMock);
    }

    [Fact(DisplayName = "Get Sale By Id - Sale Not Found")]
    [Trait("Sale", "Service")]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSaleNotFound()
    {
        // Arrange
        var (repository, branchProductRepository, validator, rabbitMQIntegration, logger) = CreateDependencies();

        repository.GetWithItemsByIdAsync(Arg.Any<int>()).Returns((Sale?)null);

        var saleService = new SaleService(repository, branchProductRepository, validator, rabbitMQIntegration, logger);

        // Act
        var result = await saleService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    private (ISaleRepository, IBranchProductRepository, IValidator<Sale>, IRabbitMQIntegration, ILogger<SaleService>) CreateDependencies()
    {
        var repository = Substitute.For<ISaleRepository>();
        var branchProductRepository = Substitute.For<IBranchProductRepository>();
        var validator = Substitute.For<IValidator<Sale>>();
        var rabbitMQIntegration = Substitute.For<IRabbitMQIntegration>();
        var logger = Substitute.For<ILogger<SaleService>>();

        return (repository, branchProductRepository, validator, rabbitMQIntegration, logger);
    }
}
