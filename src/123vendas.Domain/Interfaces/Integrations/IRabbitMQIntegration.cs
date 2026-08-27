using _123vendas.Domain.Base;

namespace _123vendas.Domain.Interfaces.Integrations;

public interface IRabbitMQIntegration
{
    public Task PublishMessageAsync(BaseEvent @event, CancellationToken cancellationToken = default);
}