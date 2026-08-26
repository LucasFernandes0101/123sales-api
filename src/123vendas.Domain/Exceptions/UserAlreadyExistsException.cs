using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class UserAlreadyExistsException(
    string message) : BaseException(message)
{
}