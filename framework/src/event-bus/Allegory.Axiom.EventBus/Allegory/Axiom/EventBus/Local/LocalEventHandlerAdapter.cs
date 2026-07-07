using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Local;

/// <summary>
/// Non-generic event handler abstraction used internally by event bus implementations
/// to store and invoke <see cref="ILocalEventHandler{T}"/> instances without knowing the payload type at compile time.
/// Expose this interface if you need to override or extend the default event bus dispatching behavior for local events.
/// </summary>
public interface ILocalEventHandlerAdapter
{
    Task HandleAsync(object payload);
}

public class LocalEventHandlerAdapter<T>(ILocalEventHandler<T> service) : ILocalEventHandlerAdapter
{
    protected internal readonly ILocalEventHandler<T> Service = service;

    public Task HandleAsync(object payload)
    {
        return Service.HandleAsync((T) payload);
    }
}