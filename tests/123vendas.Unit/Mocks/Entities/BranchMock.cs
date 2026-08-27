using _123vendas.Domain.Entities;
using Bogus;

namespace _123vendas.Unit.Mocks.Entities;

public class BranchMock : Faker<Branch>
{
    public BranchMock()
    {
        RuleFor(b => b.Id, f => f.Random.Int(1, 1000))
        .RuleFor(b => b.Name, f => f.Company.CompanyName())
        .RuleFor(b => b.Address, f => f.Address.FullAddress())
        .RuleFor(b => b.Phone, f => f.Phone.PhoneNumber())
        .RuleFor(b => b.IsActive, f => f.Random.Bool());
    }
}