using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IDistributedEventBus
{
    Task PublishAsync<T>(
        T payload,
        DistributedMessagePublishMode publishMode = DistributedMessagePublishMode.Outbox)
        where T : notnull;

    Task InitializeAsync();
}