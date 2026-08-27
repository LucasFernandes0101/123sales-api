using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using Bogus;

namespace _123vendas.Unit.Mocks.Entities;

public class SaleMock : Faker<Sale>
{
    public SaleMock()
    {
        RuleFor(s => s.Id, f => f.Random.Int(1, 1000))
        .RuleFor(s => s.Status, f => f.PickRandom<SaleStatus>())
        .RuleFor(s => s.Date, f => f.Date.Past(1))
        .RuleFor(s => s.UserId, f => f.Random.Int(1, 100))
        .RuleFor(s => s.BranchId, f => f.Random.Int(1, 10))
        .RuleFor(s => s.CancelledAt, f => f.Date.Recent())
        .RuleFor(s => s.Items, f => new Faker<SaleItem>()
            .RuleFor(i => i.ProductId, f => f.Random.Int(1, 100))
            .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(i => i.Price, f => Math.Round(f.Finance.Amount(1, 100), 2))
            .Generate(f.Random.Int(1, 5))
        );
    }
}