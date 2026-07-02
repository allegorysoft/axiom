using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.EventBus;

public static class RabbitMqEventBusOptionsExtensions
{
    extension(DistributedEventBusOptions options)
    {
        public RabbitMqEventBusOptions RabbitMq
        {
            get => options.GetOrAddProperty(
                EventBusRabbitMqPackage.RabbitMqOptionsKey,
                static () => new RabbitMqEventBusOptions());

            set => options.ExtraProperties[EventBusRabbitMqPackage.RabbitMqOptionsKey] = value;
        }
    }
}