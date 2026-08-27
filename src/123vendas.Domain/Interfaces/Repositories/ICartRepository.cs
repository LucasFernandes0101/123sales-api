using _123vendas.Domain.Base.Interfaces;
using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Repositories;

public interface ICartRepository : IBaseRepository<Cart>
{
    Task<Cart?> GetWithProductsByIdAsync(int id, CancellationToken cancellationToken = default);
}