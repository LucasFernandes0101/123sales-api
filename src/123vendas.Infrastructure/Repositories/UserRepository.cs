using _123vendas.Domain.Entities;
using _123vendas.Domain.Enums;
using _123vendas.Domain.Interfaces.Repositories;
using _123vendas.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace _123vendas.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public class UserRepository(PostgreDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email &&
                                                           u.Status == UserStatus.Active, cancellationToken);
}