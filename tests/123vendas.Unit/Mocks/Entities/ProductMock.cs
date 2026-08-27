using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using Bogus;

namespace _123vendas.Unit.Mocks.Entities;

public class ProductMock : Faker<Product>
{
    public ProductMock()
    {
        RuleFor(p => p.Id, f => f.Random.Short())
        .RuleFor(p => p.Title, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Category, f => f.PickRandom<ProductCategory>())
        .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000))
        .RuleFor(p => p.Image, f => f.Internet.Url())
        .RuleFor(p => p.IsActive, f => f.Random.Bool())
        .RuleFor(p => p.Rating, f => new ProductRating
        {
            Rate = f.Random.Double(0, 10),
            Count = f.Random.Int(0, 1000)
        });
    }
}