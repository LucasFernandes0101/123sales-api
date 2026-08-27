using _123vendas.Application.DTOs.Carts;
using _123vendas.Domain.Enums;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class CartsControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = "Carts - GET Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task GetAsync_ShouldReturnOk_WhenDataExists()
    {
        await SeedUserForCartAsync();

        var cartRequest = new CartPostRequestDTO
        {
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Carts", cartRequest);
        postResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/v1/Carts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Carts - GET Returns 204 When No Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task GetAsync_ShouldReturnNoContent_WhenNoDataExists()
    {
        var response = await _client.GetAsync("/api/v1/Carts");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Carts - GET By Id Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task GetByIdAsync_ShouldReturnOk_WhenCartExists()
    {
        await SeedUserForCartAsync();

        var cartRequest = new CartPostRequestDTO
        {
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Carts", cartRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<CartPostResponseDTO>();

        var response = await _client.GetAsync($"/api/v1/Carts/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Carts - GET By Id Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/Carts/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Carts - POST Returns 201 With Valid Payload")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task PostAsync_ShouldReturnCreated_WhenValidPayload()
    {
        await SeedUserForCartAsync();

        var request = new CartPostRequestDTO
        {
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Carts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CartPostResponseDTO>();
        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
    }

    [Fact(DisplayName = "Carts - PUT Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task PutAsync_ShouldReturnOk_WhenCartExists()
    {
        await SeedUserForCartAsync();

        var cartRequest = new CartPostRequestDTO
        {
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Carts", cartRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<CartPostResponseDTO>();

        var putRequest = new CartPutRequestDTO
        {
            Id = created!.Id,
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var response = await _client.PutAsJsonAsync($"/api/v1/Carts/{created.Id}", putRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Carts - PUT Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task PutAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        var putRequest = new CartPutRequestDTO
        {
            Id = 9999,
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var response = await _client.PutAsJsonAsync("/api/v1/Carts/9999", putRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Carts - DELETE Returns 204 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenCartExists()
    {
        await SeedUserForCartAsync();

        var cartRequest = new CartPostRequestDTO
        {
            UserId = 1,
            Date = DateTimeOffset.UtcNow,
            Products = []
        };

        var postResponse = await _client.PostAsJsonAsync("/api/v1/Carts", cartRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<CartPostResponseDTO>();

        var response = await _client.DeleteAsync($"/api/v1/Carts/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Carts - DELETE Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Carts")]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/v1/Carts/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Seed Helpers

    private async Task<int> SeedUserForCartAsync()
    {
        await _factory.SeedUsersAsync();
        return 1;
    }

    #endregion
}
