using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class EntityNotFoundException(
    string message) : BaseException(message)
{
}