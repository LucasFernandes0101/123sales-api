using _123vendas.Domain.Base.Interfaces;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;

namespace _123vendas.Domain.Interfaces.Repositories;

public interface IBranchProductRepository : IBaseRepository<BranchProduct>
{
    Task UpdateByProductIdAsync(
        int productId,
        string productName,
        ProductCategory productCategory,
        CancellationToken cancellationToken = default);
}