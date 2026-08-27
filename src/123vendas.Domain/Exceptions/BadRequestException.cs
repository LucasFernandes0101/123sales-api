using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class BadRequestException(
    string message) : BaseException(message)
{
}