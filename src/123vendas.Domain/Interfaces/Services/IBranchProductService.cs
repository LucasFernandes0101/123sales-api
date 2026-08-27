using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Services;

public interface IBranchProductService
{
    Task<PagedResult<BranchProduct>> GetAllAsync(
        int? id = default,
        int? branchId = default,
        int? productId = default,
        bool? isActive = default,
        DateTimeOffset? startDate = default,
        DateTimeOffset? endDate = default,
        int page = 1,
        int maxResults = 10,
        string? orderByClause = default,
        CancellationToken cancellationToken = default);
    Task<BranchProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BranchProduct> CreateAsync(BranchProduct request, CancellationToken cancellationToken = default);
    Task<BranchProduct> UpdateAsync(int id, BranchProduct request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}