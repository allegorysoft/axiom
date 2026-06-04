using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IDistributedEventBus
{
    Task PublishAsync<T>(T payload, bool onUnitOfWorkComplete = true, bool useOutbox = true) where T : notnull;
}