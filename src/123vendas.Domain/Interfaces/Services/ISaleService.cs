using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;

namespace _123vendas.Domain.Interfaces.Services;

public interface ISaleService
{
    Task<PagedResult<Sale>> GetAllAsync(
        int? id = default,
        int? branchId = default,
        int? userId = default,
        SaleStatus? status = default,
        DateTimeOffset? startDate = default,
        DateTimeOffset? endDate = default,
        int page = 1,
        int maxResults = 10,
        string? orderByClause = default,
        CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Sale> CreateAsync(Sale request, CancellationToken cancellationToken = default);
    Task<Sale> UpdateAsync(int saleId, Sale request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int saleId, CancellationToken cancellationToken = default);
    Task<Sale> CancelAsync(int saleId, CancellationToken cancellationToken = default);
    Task<Sale> CancelItemAsync(int saleId, int sequence, CancellationToken cancellationToken = default);
    Task<SaleItem> GetItemAsync(int saleId, int sequence, CancellationToken cancellationToken = default);
}