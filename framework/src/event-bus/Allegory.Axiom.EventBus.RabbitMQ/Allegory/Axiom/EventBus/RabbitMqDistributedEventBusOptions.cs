using Allegory.Axiom.RabbitMQ;

namespace Allegory.Axiom.EventBus;

public class RabbitMqDistributedEventBusOptions
{
    public string ConnectionName { get; set; } = RabbitMqOptions.DefaultConnectionName;
}