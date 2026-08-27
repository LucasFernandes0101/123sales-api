using _123vendas.Domain.Base.Interfaces;
using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
}