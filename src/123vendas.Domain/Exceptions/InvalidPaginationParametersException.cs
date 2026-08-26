using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class InvalidPaginationParametersException(
    string message) : BaseException(message)
{
}