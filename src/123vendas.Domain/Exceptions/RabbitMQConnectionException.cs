using _123vendas.Domain.Base;

namespace _123vendas.Domain.Exceptions;

public class RabbitMQConnectionException(
    string message) : BaseException(message)
{
}