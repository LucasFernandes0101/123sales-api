using _123vendas.Application.DTOs.BranchProducts;
using _123vendas.Domain.Entities;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class BranchProductsControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "BranchProducts - GET Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task GetAsync_ShouldReturnOk_WhenDataExists()
    {
        await SeedBranchProductAsync();

        var response = await _client.GetAsync("/api/v1/BranchProducts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BranchProducts - GET Returns 204 When No Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task GetAsync_ShouldReturnNoContent_WhenNoDataExists()
    {
        var response = await _client.GetAsync("/api/v1/BranchProducts");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "BranchProducts - GET By Id Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task GetByIdAsync_ShouldReturnOk_WhenBranchProductExists()
    {
        var (branchProduct, _, _) = await SeedBranchProductAsync();

        var response = await _client.GetAsync($"/api/v1/BranchProducts/{branchProduct.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "BranchProducts - GET By Id Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenBranchProductDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/BranchProducts/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "BranchProducts - POST Returns 401 Without Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenNoToken()
    {
        var branch = await _factory.SeedBranchAsync();
        var product = await _factory.SeedProductAsync();

        var request = new BranchProductPostRequestDTO
        {
            BranchId = branch.Id,
            ProductId = product.Id,
            Price = 50m,
            StockQuantity = 100,
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/BranchProducts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "BranchProducts - POST Returns 403 With Admin Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task PostAsync_ShouldReturnForbidden_WhenAdminRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetAdminTokenAsync(_client);
        var branch = await _factory.SeedBranchAsync();
        var product = await _factory.SeedProductAsync();

        var request = new BranchProductPostRequestDTO
        {
            BranchId = branch.Id,
            ProductId = product.Id,
            Price = 50m,
            StockQuantity = 100,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/BranchProducts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "BranchProducts - POST Returns 201 With Manager Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task PostAsync_ShouldReturnCreated_WhenManagerRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var branch = await _factory.SeedBranchAsync();
        var product = await _factory.SeedProductAsync();

        var request = new BranchProductPostRequestDTO
        {
            BranchId = branch.Id,
            ProductId = product.Id,
            Price = 50m,
            StockQuantity = 100,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/BranchProducts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<BranchProductPostResponseDTO>();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "BranchProducts - PUT Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task PutAsync_ShouldReturnOk_WhenBranchProductExists()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var (branchProduct, _, _) = await SeedBranchProductAsync();

        var request = new BranchProductPutRequestDTO
        {
            Id = branchProduct.Id,
            Price = 75m,
            StockQuantity = 200,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/BranchProducts/{branchProduct.Id}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BranchProductPutResponseDTO>();
        result.Should().NotBeNull();
        result!.Price.Should().Be(75m);
    }

    [Fact(DisplayName = "BranchProducts - PUT Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task PutAsync_ShouldReturnNotFound_WhenBranchProductDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        var request = new BranchProductPutRequestDTO
        {
            Id = 9999,
            Price = 75m,
            StockQuantity = 200,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PutAsJsonAsync("/api/v1/BranchProducts/9999", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "BranchProducts - DELETE Returns 204 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenBranchProductExists()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var (branchProduct, _, _) = await SeedBranchProductAsync();

        _client.WithToken(token);

        var response = await _client.DeleteAsync($"/api/v1/BranchProducts/{branchProduct.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "BranchProducts - DELETE Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "BranchProducts")]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenBranchProductDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/BranchProducts/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Seed Helpers

    private async Task<(BranchProduct, int, int)> SeedBranchProductAsync()
    {
        var branch = await _factory.SeedBranchAsync();
        var product = await _factory.SeedProductAsync();
        var branchProduct = await _factory.SeedBranchProductAsync(branch.Id, product.Id);

        return (branchProduct, branch.Id, product.Id);
    }

    #endregion
}
