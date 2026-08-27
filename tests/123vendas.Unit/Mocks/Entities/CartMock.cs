using _123vendas.Domain.Entities;
using Bogus;

namespace _123vendas.Unit.Mocks.Entities;

public class CartMock : Faker<Cart>
{
    public CartMock()
    {
        RuleFor(c => c.Id, f => f.Random.Int(1, 1000))
            .RuleFor(c => c.UserId, f => f.Random.Int(1, 1000))
            .RuleFor(c => c.Date, f => f.Date.RecentOffset())
            .RuleFor(c => c.Products, f => new Faker<CartProduct>()
                .RuleFor(cp => cp.CartId, f => f.Random.Int(1, 1000))
                .RuleFor(cp => cp.ProductId, f => f.Random.Int(1, 100))
                .RuleFor(cp => cp.Quantity, f => f.Random.Int(1, 10))
                .Generate(f.Random.Int(1, 5))
            );
    }
}