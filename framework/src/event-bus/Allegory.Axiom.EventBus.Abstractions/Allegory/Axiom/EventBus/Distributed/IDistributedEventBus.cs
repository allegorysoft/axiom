using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Distributed;

public interface IDistributedEventBus
{
    Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
        where T : notnull;

    Task InitializeAsync();
}