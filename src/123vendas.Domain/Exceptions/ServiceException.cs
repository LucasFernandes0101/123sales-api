using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class ServiceException(
    string message,
    Exception innerException) : BaseException(message, innerException)
{
}