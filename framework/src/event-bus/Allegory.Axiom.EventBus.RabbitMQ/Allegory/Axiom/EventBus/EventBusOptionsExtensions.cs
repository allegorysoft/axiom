using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.EventBus;

public static class EventBusOptionsExtensions
{
    extension(DistributedEventBusOptions options)
    {
        public RabbitMqEventBusOptions RabbitMq
        {
            get => options.GetOrAddProperty(
                EventBusRabbitMqPackage.Section,
                static () => new RabbitMqEventBusOptions());

            set => options.SetProperty(EventBusRabbitMqPackage.Section, value);
        }
    }
}