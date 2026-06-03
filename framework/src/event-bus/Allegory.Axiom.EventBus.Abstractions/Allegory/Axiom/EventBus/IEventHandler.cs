using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IEventHandler<in T>
{
    Task HandleAsync(T payload);
}

public interface ILocalEventHandler<in T> : IEventHandler<T> {}

public interface IDistributedEventHandler<in T> : IEventHandler<T> {}