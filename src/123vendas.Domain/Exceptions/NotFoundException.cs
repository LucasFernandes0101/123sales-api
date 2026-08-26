using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class NotFoundException(
    string message) : BaseException(message)
{
}