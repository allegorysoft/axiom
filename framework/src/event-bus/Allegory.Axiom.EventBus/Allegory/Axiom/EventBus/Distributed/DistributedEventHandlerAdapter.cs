using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Distributed;

/// <summary>
/// Non-generic event handler abstraction used internally by event bus implementations
/// to store and invoke <see cref="IDistributedEventHandler{T}"/> instances without knowing the payload type at compile time.
/// Expose this interface if you need to override or extend the default event bus dispatching behavior for distributed events.
/// </summary>
public interface IDistributedEventHandlerAdapter
{
    Task HandleAsync(object payload, EventContext context);
}

public class DistributedEventHandlerAdapter<T>(IDistributedEventHandler<T> service) : IDistributedEventHandlerAdapter
{
    protected internal readonly IDistributedEventHandler<T> Service = service;

    public Task HandleAsync(object payload, EventContext context)
    {
        return Service.HandleAsync((T) payload, context);
    }
}