using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Interfaces.Seeds;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Seeds;

[ExcludeFromCodeCoverage]
public class SaleSeeder(PostgreDbContext dbContext) : IDataSeeder
{

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Sales.AnyAsync(cancellationToken))
            return;

        var user = await dbContext.Users.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        var branch = await dbContext.Branches.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        if (user is null || branch is null) 
            return;

        var sale = new Sale
        {
            Status = SaleStatus.Created,
            Date = DateTime.UtcNow,
            UserId = user.Id,
            BranchId = branch.Id,
            TotalAmount = 100m
        };

        dbContext.Sales.Add(sale);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}