using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class UnauthorizedUserException(
    string message) : BaseException(message)
{
}