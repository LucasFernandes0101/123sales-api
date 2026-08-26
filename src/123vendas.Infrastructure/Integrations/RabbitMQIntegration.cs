using _123vendas.Domain.Base;
using _123vendas.Domain.Exceptions;
using _123vendas.Domain.Interfaces.Integrations;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace _123vendas.Infrastructure.Integrations;

[ExcludeFromCodeCoverage]
public class RabbitMQIntegration : IRabbitMQIntegration, IDisposable
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection? _persistentConnection;
    private IChannel? _channel;

    public RabbitMQIntegration()
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME") ?? "localhost",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
            VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUALHOST") ?? "/",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };
    }

    public async Task PublishMessageAsync(BaseEvent @event)
    {
        await EnsureConnectedAsync();

        string exchangeName = $"ex_{@event.Domain.ToLower()}";
        await _channel!.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true);

        string routingKey = @event.GetType().Name;
        string message = JsonConvert.SerializeObject(@event);
        byte[] body = Encoding.UTF8.GetBytes(message);

        var basicProperties = new BasicProperties { Persistent = true };

        for (int retry = 0; retry < 10; retry++)
        {
            try
            {
                await _channel!.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: body
                );
                return;
            }
            catch (AlreadyClosedException ex)
            {
                await HandlePublishErrorAsync(ex, retry, "Connection already closed.");
            }
            catch (BrokerUnreachableException ex)
            {
                await HandlePublishErrorAsync(ex, retry, "Broker unreachable.");
            }
            catch (Exception ex)
            {
                await HandlePublishErrorAsync(ex, retry, "Unknown error occurred while publishing.");
            }
        }

        throw new RabbitMQMessageException("Failed to publish message after multiple attempts.");
    }

    private async Task EnsureConnectedAsync()
    {
        if (_persistentConnection is null || !_persistentConnection.IsOpen)
            _persistentConnection = TryConnect(_connectionFactory);

        if (_channel is null || _channel.IsClosed)
            _channel = await _persistentConnection.CreateChannelAsync();
    }

    private static async Task HandlePublishErrorAsync(Exception ex, int retry, string message)
    {
        if (retry == 9)
        {
            throw new RabbitMQMessageException($"{message} Error: {ex.Message}", ex);
        }

        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)));
    }

    private static IConnection TryConnect(ConnectionFactory connectionFactory)
    {
        string errorMessage = string.Empty;

        for (int attempt = 0; attempt < 4; attempt++)
        {
            try
            {
                return connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            }
            catch (BrokerUnreachableException ex)
            {
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))).Wait();
        }

        throw new RabbitMQConnectionException($"Failed to connect to RabbitMQ after multiple attempts. Error: {errorMessage}");
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _persistentConnection?.CloseAsync().GetAwaiter().GetResult();
        _channel?.Dispose();
        _persistentConnection?.Dispose();
    }
}