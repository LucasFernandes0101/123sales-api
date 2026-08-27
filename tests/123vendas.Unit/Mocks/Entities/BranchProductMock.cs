using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using Bogus;

namespace _123vendas.Unit.Mocks.Entities;

public class BranchProductMock : Faker<BranchProduct>
{
    public BranchProductMock()
    {
        RuleFor(bp => bp.Id, f => f.IndexFaker + 1)
        .RuleFor(bp => bp.BranchId, f => f.Random.Int(1, 100))
        .RuleFor(bp => bp.ProductId, f => f.Random.Int(1, 1000))
        .RuleFor(bp => bp.ProductTitle, f => f.Commerce.ProductName())
        .RuleFor(bp => bp.ProductCategory, f => f.PickRandom<ProductCategory>())
        .RuleFor(bp => bp.Price, f => f.Finance.Amount(1, 1000, 2))
        .RuleFor(bp => bp.StockQuantity, f => f.Random.Int(100, 1000))
        .RuleFor(bp => bp.IsActive, f => f.Random.Bool());
    }
}