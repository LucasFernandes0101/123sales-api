using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Services;

public interface IBranchService
{
    Task<PagedResult<Branch>> GetAllAsync(
        int? id = default,
        bool? isActive = default,
        string? name = default,
        DateTimeOffset? startDate = default,
        DateTimeOffset? endDate = default,
        int page = 1,
        int maxResults = 10,
        string? orderByClause = default,
        CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Branch> CreateAsync(Branch request, CancellationToken cancellationToken = default);
    Task<Branch> UpdateAsync(int id, Branch request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}