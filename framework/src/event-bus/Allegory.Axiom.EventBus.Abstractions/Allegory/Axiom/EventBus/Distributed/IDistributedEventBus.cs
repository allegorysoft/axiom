using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Distributed;

public interface IDistributedEventBus
{
    Task PublishAsync<T>(
        T payload,
        DistributedMessagePublishMode publishMode = DistributedMessagePublishMode.Outbox)
        where T : notnull;

    Task InitializeAsync();
}