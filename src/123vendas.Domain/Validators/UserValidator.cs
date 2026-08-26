using _123vendas.Domain.Entities;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Domain.Validators;

[ExcludeFromCodeCoverage]
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("Email should not exceed 100 characters.");

        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username should not exceed 100 characters.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password should be at least 8 characters long.")
            .MaximumLength(200).WithMessage("Password should not exceed 200 characters.");

        RuleFor(u => u.Name)
            .NotNull().WithMessage("Name is required.")
            .DependentRules(() =>
            {
                RuleFor(u => u.Name!.Firstname)
                    .NotEmpty().WithMessage("First name is required.")
                    .MaximumLength(100).WithMessage("First name should not exceed 100 characters.");

                RuleFor(u => u.Name!.Lastname)
                    .NotEmpty().WithMessage("Last name is required.")
                    .MaximumLength(100).WithMessage("Last name should not exceed 100 characters.");
            });

        RuleFor(u => u.Address)
            .NotNull().WithMessage("Address is required.")
            .DependentRules(() =>
            {
                RuleFor(u => u.Address!.City)
                    .NotEmpty().WithMessage("City is required.")
                    .MaximumLength(100).WithMessage("City should not exceed 100 characters.");

                RuleFor(u => u.Address!.Street)
                    .NotEmpty().WithMessage("Street is required.")
                    .MaximumLength(150).WithMessage("Street should not exceed 150 characters.");

                RuleFor(u => u.Address!.Number)
                    .NotEmpty().WithMessage("Street number is required.")
                    .MaximumLength(10).WithMessage("Street number should not exceed 10 characters.");

                RuleFor(u => u.Address!.Zipcode)
                    .NotEmpty().WithMessage("Zipcode is required.")
                    .MaximumLength(20).WithMessage("Zipcode should not exceed 20 characters.");

                RuleFor(u => u.Address!.Geolocation)
                    .NotNull().WithMessage("Geolocation is required.")
                    .DependentRules(() =>
                    {
                        RuleFor(u => u.Address!.Geolocation!.Lat)
                            .NotEmpty().WithMessage("Latitude is required.")
                            .MaximumLength(20).WithMessage("Latitude should not exceed 20 characters.");

                        RuleFor(u => u.Address!.Geolocation!.Long)
                            .NotEmpty().WithMessage("Longitude is required.")
                            .MaximumLength(20).WithMessage("Longitude should not exceed 20 characters.");
                    });
            });

        RuleFor(u => u.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20).WithMessage("Phone number should not exceed 20 characters.");

        RuleFor(u => u.Status)
                .IsInEnum().WithMessage("Invalid status.");

        RuleFor(u => u.Role)
                .IsInEnum().WithMessage("Invalid role.");

        RuleFor(u => u.CreatedAt)
            .NotNull().WithMessage("CreatedAt is required.");

        RuleFor(u => u.UpdatedAt)
            .NotNull().WithMessage("UpdatedAt is required.");
    }
}