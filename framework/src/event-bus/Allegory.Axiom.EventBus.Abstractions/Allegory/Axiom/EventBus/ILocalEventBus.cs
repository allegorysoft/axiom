using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface ILocalEventBus
{
    Task PublishAsync<T>(T payload, bool onUnitOfWorkComplete = true) where T : notnull;
}