using _123vendas.Application.DTOs.Branches;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class BranchesControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{

    [Fact(DisplayName = "Branches - GET Returns 200 With Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task GetAsync_ShouldReturnOk_WhenDataExists()
    {
        await _factory.SeedBranchAsync("Branch 1");

        var response = await _client.GetAsync("/api/v1/Branches");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Branch 1");
    }

    [Fact(DisplayName = "Branches - GET Returns 204 When No Data")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task GetAsync_ShouldReturnNoContent_WhenNoDataExists()
    {
        var response = await _client.GetAsync("/api/v1/Branches");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Branches - GET By Id Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task GetByIdAsync_ShouldReturnOk_WhenBranchExists()
    {
        var branch = await _factory.SeedBranchAsync("Branch Found");

        var response = await _client.GetAsync($"/api/v1/Branches/{branch.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Branch Found");
    }

    [Fact(DisplayName = "Branches - GET By Id Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenBranchDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/Branches/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Branches - POST Returns 401 Without Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task PostAsync_ShouldReturnUnauthorized_WhenNoToken()
    {
        var request = new BranchPostRequestDTO
        {
            Name = "New Branch",
            Address = "123 Test St",
            Phone = "555555555",
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Branches", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Branches - POST Returns 403 With Admin Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task PostAsync_ShouldReturnForbidden_WhenAdminRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetAdminTokenAsync(_client);

        var request = new BranchPostRequestDTO
        {
            Name = "New Branch",
            Address = "123 Test St",
            Phone = "555555555",
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/Branches", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Branches - POST Returns 201 With Manager Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task PostAsync_ShouldReturnCreated_WhenManagerRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        var request = new BranchPostRequestDTO
        {
            Name = "New Branch",
            Address = "123 Test St",
            Phone = "555555555",
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/Branches", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<BranchPostResponseDTO>();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "Branches - POST Returns 400 With Invalid Payload")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task PostAsync_ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        var request = new BranchPostRequestDTO
        {
            Name = "",
            Address = "",
            Phone = "",
            IsActive = false
        };

        _client.WithToken(token);

        var response = await _client.PostAsJsonAsync("/api/v1/Branches", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Branches - PUT Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task PutAsync_ShouldReturnOk_WhenBranchExists()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var branch = await _factory.SeedBranchAsync("Original Branch");

        var request = new BranchPutRequestDTO
        {
            Id = branch.Id,
            Name = "Updated Branch",
            Address = "Updated Address",
            Phone = "999999999",
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PutAsJsonAsync($"/api/v1/Branches/{branch.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BranchPutResponseDTO>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Branch");
    }

    [Fact(DisplayName = "Branches - PUT Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task PutAsync_ShouldReturnNotFound_WhenBranchDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        var request = new BranchPutRequestDTO
        {
            Id = 9999,
            Name = "Updated Branch",
            Address = "Updated Address",
            Phone = "999999999",
            IsActive = true
        };

        _client.WithToken(token);

        var response = await _client.PutAsJsonAsync("/api/v1/Branches/9999", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Branches - DELETE Returns 204 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenBranchExists()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        var branch = await _factory.SeedBranchAsync("Branch To Delete");

        _client.WithToken(token);

        var response = await _client.DeleteAsync($"/api/v1/Branches/{branch.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Branches - DELETE Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Branches")]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenBranchDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);

        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/Branches/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
