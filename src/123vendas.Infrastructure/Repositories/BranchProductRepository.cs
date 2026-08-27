using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class BranchProductRepository(PostgreDbContext context) : BaseRepository<BranchProduct>(context), IBranchProductRepository
{
    public async Task UpdateByProductIdAsync(
        int productId,
        string productName,
        ProductCategory productCategory,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<BranchProduct>()
            .Where(bp => bp.ProductId == productId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(bp => bp.ProductTitle, productName)
                .SetProperty(bp => bp.ProductCategory, productCategory), cancellationToken);
    }
}