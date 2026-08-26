using _123vendas.Domain.Entities;
using _123vendas.Domain.Interfaces.Seeds;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Seeds;

[ExcludeFromCodeCoverage]
public class BranchSeeder(PostgreDbContext dbContext) : IDataSeeder
{

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Branches.AnyAsync(cancellationToken))
            return;

        var branches = new List<Branch>
        {
            new() { Name = "Matriz", Address = "Endereço Matriz", Phone = "333333333", IsActive = true },
            new() { Name = "Filial", Address = "Endereço Filial", Phone = "444444444", IsActive = true }
        };

        dbContext.Branches.AddRange(branches);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}