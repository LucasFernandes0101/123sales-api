using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using Bogus;

namespace _123vendas.Unit.Mocks.Entities;

public class UserMock : Faker<User>
{
    public UserMock()
    {
        RuleFor(u => u.Id, f => f.Random.Int(1, 1000));
        RuleFor(u => u.Email, f => f.Internet.Email());
        RuleFor(u => u.Username, f => f.Internet.UserName());
        RuleFor(u => u.Password, f => f.Internet.Password());
        RuleFor(u => u.Name, f => new UserName
        {
            Firstname = f.Name.FirstName(),
            Lastname = f.Name.LastName()
        });
        RuleFor(u => u.Address, f => new UserAddress
        {
            City = f.Address.City(),
            Street = f.Address.StreetName(),
            Number = f.Address.BuildingNumber(),
            Zipcode = f.Address.ZipCode(),
            Geolocation = new UserAddressGeolocation
            {
                Lat = f.Address.Latitude().ToString(),
                Long = f.Address.Longitude().ToString()
            }
        });
        RuleFor(u => u.Phone, f => f.Phone.PhoneNumber());
        RuleFor(u => u.Status, f => f.PickRandom<UserStatus>());
        RuleFor(u => u.Role, f => f.PickRandom<UserRole>());
        RuleFor(u => u.CreatedAt, f => f.Date.Past(1));
        RuleFor(u => u.UpdatedAt, f => f.Date.Recent(0));
    }
}