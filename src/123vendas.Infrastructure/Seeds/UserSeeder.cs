using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Domain.Interfaces.Seeds;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Seeds;

[ExcludeFromCodeCoverage]
public class UserSeeder(
    PostgreDbContext dbContext,
    IPasswordHasher passwordHasher) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
            return;

        var users = new List<User>
        {
            new()
            {
                Email = "admin@123vendas.com",
                Username = "admin",
                Password = passwordHasher.HashPassword("admin123"),
                Name = new()
                {
                    Firstname = "Admin",
                    Lastname = "User"
                },
                Address = new()
                {
                    City = "Cidade Exemplo",
                    Street = "Rua Principal",
                    Number = "123",
                    Zipcode = "00000",
                    HasAddress = true
                },
                Phone = "111111111",
                Status = UserStatus.Active,
                Role = UserRole.Admin
            },
            new()
            {
                Email = "vendedor@123vendas.com",
                Username = "seller",
                Password = passwordHasher.HashPassword("seller123"),
                Name = new()
                {
                    Firstname = "Vendedor",
                    Lastname = "User"
                },
                Address = new()
                {
                    City = "Cidade Exemplo",
                    Street = "Rua Secundária",
                    Number = "456",
                    Zipcode = "11111",
                    HasAddress = true
                },
                Phone = "222222222",
                Status = UserStatus.Active,
                Role = UserRole.Manager
            }
        };

        await dbContext.Users.AddRangeAsync(users, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}