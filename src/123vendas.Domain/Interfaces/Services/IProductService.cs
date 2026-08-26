using _123vendas.Domain.Base;
using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Services;

public interface IProductService
{
    Task<PagedResult<Product>> GetAllAsync(int? id = default, bool? isActive = default, string? title = default, string? category = default, decimal? minPrice = default, decimal? maxPrice = default, DateTimeOffset? startDate = default, DateTimeOffset? endDate = default, int page = 1, int maxResults = 10, string? orderByClause = default);
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product request);
    Task<Product> UpdateAsync(int id, Product request);
    Task DeleteAsync(int id);
    IEnumerable<string> GetAllCategories();
}