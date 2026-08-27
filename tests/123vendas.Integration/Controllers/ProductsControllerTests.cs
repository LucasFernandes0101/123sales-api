using _123vendas.Application.DTOs.Products;
using _123vendas.Domain.Enums;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class ProductsControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{

    [Fact(DisplayName = "Products - GET Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetAsync_ShouldReturnOk_WhenDataExists()
    {
        await _factory.SeedProductAsync("Test Beer");

        var response = await _client.GetAsync("/api/v1/Products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Beer");
    }

    [Fact(DisplayName = "Products - GET Returns 204 When No Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetAsync_ShouldReturnNoContent_WhenNoDataExists()
    {
        var response = await _client.GetAsync("/api/v1/Products");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Products - GET Categories Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetCategoriesAsync_ShouldReturnOk_WhenCategoriesExist()
    {
        await _factory.SeedProductAsync("Beer Product");

        var response = await _client.GetAsync("/api/v1/Products/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Products - GET Categories Returns 200 With Enum Values")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetCategoriesAsync_ShouldReturnOk_WithCategories()
    {
        var response = await _client.GetAsync("/api/v1/Products/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await response.Content.ReadFromJsonAsync<List<string>>();
        categories.Should().NotBeNull();
        categories.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Products - GET By Id Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetByIdAsync_ShouldReturnOk_WhenProductExists()
    {
        var product = await _factory.SeedProductAsync("Found Product");

        var response = await _client.GetAsync($"/api/v1/Products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Found Product");
    }

    [Fact(DisplayName = "Products - GET By Id Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/Products/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Products - GET By Category Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetByCategoryAsync_ShouldReturnOk_WhenProductsExistInCategory()
    {
        await _factory.SeedProductAsync("Beer 1");

        var response = await _client.GetAsync("/api/v1/Products/category/Beer");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Products - GET By Category Returns 204 When No Products")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task GetByCategoryAsync_ShouldReturnNoContent_WhenNoProductsInCategory()
    {
        var response = await _client.GetAsync("/api/v1/Products/category/Beer");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Products - POST Returns 401 Without Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenNoToken()
    {
        var request = new ProductPostRequestDTO
        {
            Title = "New Product",
            Description = "Description",
            Category = ProductCategory.Beer,
            Price = 15.99m,
            Image = "img.jpg",
            Rating = 4.5,
            RateCount = 10,
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Products - POST Returns 403 With Admin Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task PostAsync_ShouldReturnForbidden_WhenAdminRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetAdminTokenAsync(_client);

        var request = new ProductPostRequestDTO
        {
            Title = "New Product",
            Description = "Description",
            Category = ProductCategory.Beer,
            Price = 15.99m,
            Image = "img.jpg",
            Rating = 4.5,
            RateCount = 10,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Products - POST Returns 201 With Manager Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task PostAsync_ShouldReturnCreated_WhenManagerRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        var request = new ProductPostRequestDTO
        {
            Title = "New Product",
            Description = "Description",
            Category = ProductCategory.Beer,
            Price = 15.99m,
            Image = "img.jpg",
            Rating = 4.5,
            RateCount = 10,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/Products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ProductPostResponseDTO>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Product");
    }

    [Fact(DisplayName = "Products - PUT Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task PutAsync_ShouldReturnOk_WhenProductExists()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var product = await _factory.SeedProductAsync("Original Product");

        var request = new ProductPutRequestDTO
        {
            Id = product.Id,
            Title = "Updated Product",
            Description = "Updated Description",
            Category = ProductCategory.Soda,
            Price = 20.99m,
            Image = "updated.jpg",
            Rating = 4.8,
            RateCount = 15,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PutAsJsonAsync($"/api/v1/Products/{product.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ProductPutResponseDTO>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Product");
    }

    [Fact(DisplayName = "Products - PUT Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task PutAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        var request = new ProductPutRequestDTO
        {
            Id = 9999,
            Title = "Updated Product",
            Description = "Updated Description",
            Category = ProductCategory.Soda,
            Price = 20.99m,
            Image = "updated.jpg",
            Rating = 4.8,
            RateCount = 15,
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PutAsJsonAsync("/api/v1/Products/9999", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Products - DELETE Returns 204 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenProductExists()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var product = await _factory.SeedProductAsync("Product To Delete");

        _client.WithToken(token);

        var response = await _client.DeleteAsync($"/api/v1/Products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Products - DELETE Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Products")]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/Products/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
