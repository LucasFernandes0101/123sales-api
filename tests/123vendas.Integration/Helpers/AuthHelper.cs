using _123vendas.Application.DTOs.Auth;

namespace _123vendas.Integration.Helpers;

public static class AuthHelper
{
    public const string AdminEmail = "admin@123vendas.com";
    public const string AdminPassword = "admin123";
    public const string ManagerEmail = "vendedor@123vendas.com";
    public const string ManagerPassword = "seller123";

    public static async Task<string> GetAdminTokenAsync(HttpClient client)
        => await GetTokenAsync(client, AdminEmail, AdminPassword);

    public static async Task<string> GetManagerTokenAsync(HttpClient client)
        => await GetTokenAsync(client, ManagerEmail, ManagerPassword);

    public static async Task<string> GetTokenAsync(
        HttpClient client,
        string email,
        string password)
    {
        var request = new AuthenticateUserRequestDTO
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("/api/v1/Auth", request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthenticateUserResponseDTO>();

        return result?.Token
            ?? throw new InvalidOperationException("Failed to obtain JWT token.");
    }
}
