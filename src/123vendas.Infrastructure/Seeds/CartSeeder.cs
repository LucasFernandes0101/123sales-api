using _123vendas.Domain.Entities;
using _123vendas.Domain.Interfaces.Seeds;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Seeds;

[ExcludeFromCodeCoverage]
public class CartSeeder(PostgreDbContext dbContext) : IDataSeeder
{

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Carts.AnyAsync(cancellationToken))
            return;

        var user = await dbContext.Users.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (user == null) return;

        var cart = new Cart
        {
            UserId = user.Id,
            Date = DateTime.UtcNow,
        };

        dbContext.Carts.Add(cart);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}