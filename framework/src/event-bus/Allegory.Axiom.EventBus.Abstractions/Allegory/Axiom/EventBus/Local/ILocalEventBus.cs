using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Local;

public interface ILocalEventBus
{
    Task PublishAsync<T>(
        T payload,
        LocalEventPublishMode publishMode = LocalEventPublishMode.OnUnitOfWorkComplete)
        where T : notnull;
}