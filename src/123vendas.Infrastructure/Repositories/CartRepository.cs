using _123vendas.Domain.Entities;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class CartRepository(PostgreDbContext context) : BaseRepository<Cart>(context), ICartRepository
{
    public async Task<Cart?> GetWithProductsByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbContext.Carts.Include(s => s.Products)
                                 .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}