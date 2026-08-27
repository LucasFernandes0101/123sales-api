using _123vendas.Application.DTOs.Users;
using _123vendas.Domain.Enums;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class UsersControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{

    [Fact(DisplayName = "Users - POST Returns 201 With Valid Payload")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task PostAsync_ShouldReturnCreated_WhenValidPayload()
    {
        var request = new UserPostRequestDTO
        {
            Username = "newuser",
            Password = "Password@123",
            Phone = "333333333",
            Name = new() { Firstname = "New", Lastname = "User" },
            Address = new()
            {
                City = "Test City",
                Street = "Test St",
                Number = "100",
                Zipcode = "00000"
            },
            Email = "newuser@test.com",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<UserPostResponseDTO>();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "Users - POST Returns 400 With Invalid Payload")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task PostAsync_ShouldReturnBadRequest_WhenPayloadIsInvalid()
    {
        var request = new UserPostRequestDTO
        {
            Username = "",
            Password = "",
            Email = "",
            Phone = "",
            Name = null,
            Address = null,
            Status = UserStatus.Unknown,
            Role = UserRole.None
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Users - POST Returns 409 When Email Already Exists")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task PostAsync_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        await _factory.SeedUsersAsync();

        var request = new UserPostRequestDTO
        {
            Username = "admin2",
            Password = "Password@123",
            Phone = "333333333",
            Name = new() { Firstname = "Admin", Lastname = "User" },
            Address = new()
            {
                City = "Test City",
                Street = "Test St",
                Number = "100",
                Zipcode = "00000"
            },
            Email = "admin@123vendas.com",
            Status = UserStatus.Active,
            Role = UserRole.Admin
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Users - GET By Id Returns 200 When Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task GetByIdAsync_ShouldReturnOk_WhenUserExists()
    {
        await _factory.SeedUsersAsync();

        var response = await _client.GetAsync("/api/v1/Users/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Users - GET By Id Returns 404 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1/Users/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Users - DELETE Returns 401 Without Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task DeleteAsync_ShouldReturnUnauthorized_WhenNoToken()
    {
        await _factory.SeedUsersAsync();

        var response = await _client.DeleteAsync("/api/v1/Users/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Users - DELETE Returns 403 With Admin Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task DeleteAsync_ShouldReturnForbidden_WhenAdminRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetAdminTokenAsync(_client);
        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/Users/2");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Users - DELETE Returns 204 With Manager Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenManagerRole()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/Users/1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Users - DELETE Returns 204 When Not Found")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Users")]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenUserDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var token = await AuthHelper.GetManagerTokenAsync(_client);
        _client.WithToken(token);

        var response = await _client.DeleteAsync("/api/v1/Users/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
