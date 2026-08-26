using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class SaleAlreadyCanceledException(
    string message) : BaseException(message)
{
}
