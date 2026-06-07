using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface ILocalEventBus
{
    Task PublishAsync<T>(
        T payload,
        DispatchMode dispatchMode = DispatchMode.OnUnitOfWorkComplete) 
        where T : notnull;
}