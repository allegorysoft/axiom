using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Local;

public interface ILocalEventBus
{
    const string UnitOfWorkItemKey = $"{nameof(ILocalEventBus)}";

    Task PublishAsync<T>(
        T payload,
        LocalEventPublishMode publishMode = LocalEventPublishMode.OnUnitOfWorkComplete)
        where T : notnull;
}