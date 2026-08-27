using _123vendas.Application.DTOs.Auth;
using _123vendas.Integration.Fixtures;
using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Controllers;

public class AuthControllerTests(IntegrationTestFactory factory) : BaseIntegrationTest(factory)
{

    [Fact(DisplayName = "Auth - Valid Credentials Returns Token")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Auth")]
    public async Task AuthenticateUser_ShouldReturnToken_WhenValidCredentials()
    {
        await _factory.SeedUsersAsync();

        var request = new AuthenticateUserRequestDTO
        {
            Email = AuthHelper.AdminEmail,
            Password = AuthHelper.AdminPassword
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthenticateUserResponseDTO>();

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(AuthHelper.AdminEmail);
        result.Username.Should().Be("admin");
        result.Role.Should().Be("Admin");
    }

    [Fact(DisplayName = "Auth - Invalid Email Returns 400")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Auth")]
    public async Task AuthenticateUser_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        await _factory.SeedUsersAsync();

        var request = new AuthenticateUserRequestDTO
        {
            Email = "",
            Password = "somepassword"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Auth - Wrong Password Returns 401")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Auth")]
    public async Task AuthenticateUser_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        await _factory.SeedUsersAsync();

        var request = new AuthenticateUserRequestDTO
        {
            Email = AuthHelper.AdminEmail,
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Auth - Nonexistent User Returns 401")]
    [Trait("Category", "Integration")]
    [Trait("Controller", "Auth")]
    public async Task AuthenticateUser_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        await _factory.SeedUsersAsync();

        var request = new AuthenticateUserRequestDTO
        {
            Email = "nonexistent@test.com",
            Password = "somepassword"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/Auth", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
