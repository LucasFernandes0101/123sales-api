using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class SaleItemAlreadyCanceledException(
    string message) : BaseException(message)
{
}
