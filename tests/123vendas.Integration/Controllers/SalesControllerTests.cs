using _123vendas.Application.DTOs.Sales;
using _123vendas.Domain.Enums;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class SalesControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "Sales - GET Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task GetAsync_ShouldReturnOk_WhenDataExists()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items =
            [
                new() { ProductId = productId, Quantity = 1 }
            ]
        };

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        postResponse.EnsureSuccessStatusCode();

        _client.WithoutToken();

        var response = await _client.GetAsync("/api/v1/Sales");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Sales - GET Returns 204 When No Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task GetAsync_ShouldReturnNoContent_WhenNoDataExists()
    {
        var response = await _client.GetAsync("/api/v1/Sales");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Sales - GET By Id Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task GetByIdAsync_ShouldReturnOk_WhenSaleExists()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<SalePostResponseDTO>();

        _client.WithoutToken();

        var response = await _client.GetAsync($"/api/v1/Sales/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Sales - GET By Id Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/Sales/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Sales - POST Returns 401 Without Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenNoToken()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var request = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Sales - POST Returns 403 With Admin Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task PostAsync_ShouldReturnForbidden_WhenAdminRole()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetAdminTokenAsync(_client);
        _client.WithToken(token);

        var request = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Sales - POST Returns 201 With Manager Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task PostAsync_ShouldReturnCreated_WhenManagerRole()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var request = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<SalePostResponseDTO>();
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "Sales - PUT Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task PutAsync_ShouldReturnOk_WhenSaleExists()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<SalePostResponseDTO>();

        var putRequest = new SalePutRequestDTO
        {
            Id = created!.Id,
            Status = SaleStatus.Created,
            Date = DateTimeOffset.UtcNow,
            UserId = userId,
            BranchId = branchId,
            TotalAmount = 100m,
            CancelledAt = null,
            Items = [new() { ProductId = productId, Quantity = 2 }]
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/Sales/{created.Id}", putRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Sales - PUT Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task PutAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var putRequest = new SalePutRequestDTO
        {
            Id = 9999,
            Status = SaleStatus.Created,
            Date = DateTimeOffset.UtcNow,
            UserId = userId,
            BranchId = branchId,
            TotalAmount = 100m,
            CancelledAt = null,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var response = await _client.PutAsJsonAsync("/api/v1/Sales/9999", putRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Sales - DELETE Returns 204 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenSaleExists()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<SalePostResponseDTO>();

        var response = await _client.DeleteAsync($"/api/v1/Sales/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Sales - DELETE Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/Sales/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Sales - Cancel Returns 204 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task CancelAsync_ShouldReturnNoContent_WhenSaleExists()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<SalePostResponseDTO>();

        var response = await _client.PutAsync(
            $"/api/v1/Sales/{created!.Id}/cancel",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Sales - Cancel Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task CancelAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        var response = await _client.PutAsync("/api/v1/Sales/9999/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Sales - Cancel Item Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task CancelItemAsync_ShouldReturnOk_WhenSaleAndItemExist()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<SalePostResponseDTO>();

        var response = await _client.PutAsync(
            $"/api/v1/Sales/{created!.Id}/Items/1/cancel",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Sales - Cancel Item Returns 404 When Sale Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task CancelItemAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        var response = await _client.PutAsync(
            "/api/v1/Sales/9999/Items/1/cancel",
            content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Sales - GET Item Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task GetItemAsync_ShouldReturnOk_WhenSaleAndItemExist()
    {
        var (branchId, productId, userId) = await SeedSaleDependenciesAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var saleRequest = new SalePostRequestDTO
        {
            UserId = userId,
            BranchId = branchId,
            Items = [new() { ProductId = productId, Quantity = 1 }]
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Sales", saleRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<SalePostResponseDTO>();

        _client.WithoutToken();

        var response = await _client.GetAsync($"/api/v1/Sales/{created!.Id}/Items/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Sales - GET Item Returns 404 When Sale Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Sales")]
    public async Task GetItemAsync_ShouldReturnNotFound_WhenSaleDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/Sales/9999/Items/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Seed Helpers

    private async Task<(int branchId, int productId, int userId)> SeedSaleDependenciesAsync()
    {
        await _factory.SeedUsersAsync();
        var branch = await _factory.SeedBranchAsync();
        var product = await _factory.SeedProductAsync();
        await _factory.SeedBranchProductAsync(branch.Id, product.Id, price: 100m, stockQuantity: 50);

        return (branch.Id, product.Id, 1);
    }

    #endregion
}
