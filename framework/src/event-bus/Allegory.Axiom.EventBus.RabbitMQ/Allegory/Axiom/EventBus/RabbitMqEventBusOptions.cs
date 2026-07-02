using Allegory.Axiom.RabbitMQ;

namespace Allegory.Axiom.EventBus;

public class RabbitMqEventBusOptions
{
    public string ConnectionName { get; set; } = RabbitMqOptions.DefaultConnectionName;
    public string? ExchangeName { get; set; }
}