using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Interfaces.Common;
using _123vendas.Infrastructure.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace _123vendas.Integration.Fixtures;

public sealed class IntegrationTestFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("vendas_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .Build();

    public string PostgresConnectionString => _postgresContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = PostgresConnectionString
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<PostgreDbContext>>();
            services.AddDbContext<PostgreDbContext>(options =>
                options.UseNpgsql(PostgresConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        Environment.SetEnvironmentVariable("JWT_SECRETKEY",
            "ThisIsAVeryLongSecretKeyForTestingPurposesOnlyAtLeast64Chars!!");
        Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING",
            PostgresConnectionString);
        Environment.SetEnvironmentVariable("RABBITMQ_HOSTNAME",
            _rabbitMqContainer.Hostname);
        Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
        Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
        Environment.SetEnvironmentVariable("RABBITMQ_VIRTUALHOST", "/");
        Environment.SetEnvironmentVariable("SEED_DATABASE_FLAG", "false");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task SeedUsersAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await dbContext.Users.AnyAsync())
            return;

        var users = new List<User>
        {
            new()
            {
                Email = "admin@123vendas.com",
                Username = "admin",
                Password = passwordHasher.HashPassword("admin123"),
                Name = new() { Firstname = "Admin", Lastname = "User" },
                Address = new()
                {
                    City = "Test City",
                    Street = "Test St",
                    Number = "1",
                    Zipcode = "00000",
                    HasAddress = true
                },
                Phone = "111111111",
                Status = UserStatus.Active,
                Role = UserRole.Admin
            },
            new()
            {
                Email = "vendedor@123vendas.com",
                Username = "seller",
                Password = passwordHasher.HashPassword("seller123"),
                Name = new() { Firstname = "Seller", Lastname = "User" },
                Address = new()
                {
                    City = "Test City",
                    Street = "Test St",
                    Number = "2",
                    Zipcode = "11111",
                    HasAddress = true
                },
                Phone = "222222222",
                Status = UserStatus.Active,
                Role = UserRole.Manager
            }
        };

        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Branch> SeedBranchAsync(string name = "Test Branch")
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();

        var branch = new Branch
        {
            Name = name,
            Address = "Test Address",
            Phone = "999999999",
            IsActive = true
        };

        await dbContext.Branches.AddAsync(branch);
        await dbContext.SaveChangesAsync();

        return branch;
    }

    public async Task<Product> SeedProductAsync(string title = "Test Product")
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();

        var product = new Product
        {
            Title = title,
            Description = "Test Description",
            Category = ProductCategory.Beer,
            Price = 10.50m,
            Image = "test.jpg",
            Rating = new() { Rate = 4.5, Count = 10 },
            IsActive = true
        };

        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    public async Task<BranchProduct> SeedBranchProductAsync(
        int branchId,
        int productId,
        decimal price = 100m,
        int stockQuantity = 50)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgreDbContext>();

        var branchProduct = new BranchProduct
        {
            BranchId = branchId,
            ProductId = productId,
            ProductTitle = "Test Product",
            ProductCategory = ProductCategory.Beer,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true
        };

        await dbContext.BranchProducts.AddAsync(branchProduct);
        await dbContext.SaveChangesAsync();

        return branchProduct;
    }
}
