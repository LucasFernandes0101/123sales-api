using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class RabbitMQMessageException(
    string message,
    Exception? innerException = null) : BaseException(message, innerException)
{
}