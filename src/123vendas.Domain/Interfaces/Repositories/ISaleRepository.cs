using _123vendas.Domain.Base.Interfaces;
using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Repositories;

public interface ISaleRepository : IBaseRepository<Sale>
{
    Task<Sale?> GetWithItemsByIdAsync(int id, CancellationToken cancellationToken = default);
}