using _123vendas.Domain.Entities;
using _123vendas.Domain.Interfaces.Seeds;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Seeds;

[ExcludeFromCodeCoverage]
public class SaleItemSeeder(PostgreDbContext dbContext) : IDataSeeder
{

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.SaleItems.AnyAsync(cancellationToken))
            return;

        var sale = await dbContext.Sales.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        var product = await dbContext.Products.FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (sale is null || product is null) 
            return;

        var saleItem = new SaleItem
        {
            SaleId = sale.Id,
            ProductId = product.Id,
            Sequence = 1,
            ProductTitle = product.Title,
            Quantity = 1,
            UnitPrice = product.Price,
            Price = product.Price,
            Discount = 0,
            IsCancelled = false
        };

        dbContext.SaleItems.Add(saleItem);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}