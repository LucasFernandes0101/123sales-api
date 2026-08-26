using _123vendas.Domain.Entities;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace _123vendas.Application.Common.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    public async Task<string> GenerateTokenAsync(User user)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRETKEY")
                            ?? throw new InvalidOperationException("SecretKey is missing!");

        if (user is null)
            throw new BadRequestException("User is required.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new[]
        {
           new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
           new Claim(ClaimTypes.Name, user.Username!),
           new Claim(ClaimTypes.Role, user.Role.ToString())
       };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return await Task.FromResult(tokenString);
    }
}