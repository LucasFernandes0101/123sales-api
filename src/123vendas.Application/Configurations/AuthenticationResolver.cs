using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace _123vendas.Application.Configurations;

public static class AuthenticationResolver
{
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRETKEY")
            ?? throw new InvalidOperationException("SecretKey is missing!");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("ManagerOnly", policy =>
                policy.RequireRole("Manager"));

        return services;
    }
}