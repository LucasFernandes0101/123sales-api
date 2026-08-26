using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class ItemQuantityLimitExceededException(
    string message) : BaseException(message)
{
}