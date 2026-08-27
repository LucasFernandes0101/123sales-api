using _123vendas.Application.Commands.Auth;
using _123vendas.Application.Commands.Users;
using _123vendas.Application.Common.Security;
using _123vendas.Application.Mappers.Branches;
using _123vendas.Application.Mappers.BranchProducts;
using _123vendas.Application.Mappers.Carts;
using _123vendas.Application.Mappers.Products;
using _123vendas.Application.Mappers.Sales;
using _123vendas.Application.Mappers.Users;
using _123vendas.Application.Services;
using _123vendas.Application.Validators.Auth;
using _123vendas.Application.Validators.Users;
using _123vendas.Domain.Entities;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Domain.Interfaces.Integrations;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Domain.Interfaces.Seeds;
using _123vendas.Domain.Interfaces.Services;
using _123vendas.Domain.Validators;
using _123vendas.Infrastructure.Contexts;
using _123vendas.Infrastructure.Integrations;
using _123vendas.Infrastructure.Repositories;
using _123vendas.Infrastructure.Seeds;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace _123vendas.Application.Configurations;

[ExcludeFromCodeCoverage]
public static class DependencyResolver
{
    public static IServiceCollection ResolveDependencies(this IServiceCollection services)
    {
        services.ResolveFluentValidators();
        services.ResolveRepositories();
        services.ResolveServices();
        services.ResolveMediatR();
        services.ResolveCommons();
        services.ResolveSeeds();

        services.AddSingleton<IRabbitMQIntegration, RabbitMQIntegration>();

        return services;
    }

    private static void ResolveRepositories(this IServiceCollection services)
    {
        services.AddDbContext<PostgreDbContext>(options =>
            options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"), npgsqlOptions =>
                npgsqlOptions.CommandTimeout(180))
            .EnableSensitiveDataLogging(true));

        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IBranchProductRepository, BranchProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleItemRepository, SaleItemRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartProductRepository, CartProductRepository>();
    }

    private static void ResolveServices(this IServiceCollection services)
    {
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IBranchProductService, BranchProductService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ICartService, CartService>();
    }

    private static void ResolveFluentValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<Branch>, BranchValidator>();
        services.AddScoped<IValidator<BranchProduct>, BranchProductValidator>();
        services.AddScoped<IValidator<Product>, ProductValidator>();
        services.AddScoped<IValidator<ProductRating>, ProductRatingValidator>();
        services.AddScoped<IValidator<Sale>, SaleValidator>();
        services.AddScoped<IValidator<SaleItem>, SaleItemValidator>();
        services.AddScoped<IValidator<Cart>, CartValidator>();
        services.AddScoped<IValidator<CartProduct>, CartProductValidator>();

        #region Users
        services.AddScoped<IValidator<User>, UserValidator>();
        services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        services.AddScoped<IValidator<GetUserCommand>, GetUserCommandValidator>();
        services.AddScoped<IValidator<DeleteUserCommand>, DeleteUserCommandValidator>();
        services.AddScoped<IValidator<AuthenticateUserCommand>, AuthenticateUserCommandValidator>();
        #endregion
    }

    private static void ResolveSeeds(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, UserSeeder>();
        services.AddScoped<IDataSeeder, BranchSeeder>();
        services.AddScoped<IDataSeeder, ProductSeeder>();
        services.AddScoped<IDataSeeder, BranchProductSeeder>();
        services.AddScoped<IDataSeeder, CartSeeder>();
        services.AddScoped<IDataSeeder, CartProductSeeder>();
        services.AddScoped<IDataSeeder, SaleSeeder>();
        services.AddScoped<IDataSeeder, SaleItemSeeder>();

        services.AddScoped<ISeedManager, SeedManager>();
    }

    private static void ResolveCommons(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    }

    private static void ResolveMediatR(this IServiceCollection services)
     => services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
}