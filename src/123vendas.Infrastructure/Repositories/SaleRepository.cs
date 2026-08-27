using _123vendas.Domain.Entities;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class SaleRepository(PostgreDbContext context) : BaseRepository<Sale>(context), ISaleRepository
{
    public async Task<Sale?> GetWithItemsByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbContext.Sales.Include(s => s.Items)
                                 .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}