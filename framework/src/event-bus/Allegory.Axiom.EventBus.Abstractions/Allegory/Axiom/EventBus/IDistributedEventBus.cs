using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IDistributedEventBus
{
    Task PublishAsync<T>(
        T payload,
        DispatchMode dispatchMode = DispatchMode.OnUnitOfWorkComplete,
        DeliveryMode deliveryMode = DeliveryMode.Outbox)
        where T : notnull;
}