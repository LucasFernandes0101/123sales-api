using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class EntityAlreadyDeletedException(
    string message) : BaseException(message)
{
}