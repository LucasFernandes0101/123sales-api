using _123vendas.Domain.Entities;

namespace _123vendas.Domain.Interfaces.Common;

public interface IJwtTokenGenerator
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}