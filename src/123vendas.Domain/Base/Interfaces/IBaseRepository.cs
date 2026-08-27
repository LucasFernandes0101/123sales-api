using System.Linq.Expressions;

namespace _123vendas.Domain.Base.Interfaces;

public interface IBaseRepository<T>
{
    Task<PagedResult<T>> GetAsync(int page = 1, int maxResults = 10, Expression<Func<T, bool>>? criteria = default, string? orderByClause = default, CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}