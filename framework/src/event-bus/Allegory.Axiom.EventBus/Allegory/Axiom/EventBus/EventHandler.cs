using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

/// <summary>
/// Non-generic event handler abstraction used internally by event bus implementations
/// to store and invoke <see cref="IEventHandler{T}"/> instances without knowing the payload type at compile time.
/// Expose this interface if you need to override or extend the default event bus dispatching behavior.
/// </summary>
public interface IEventHandler
{
    Task HandleAsync(object payload);
}

/// <summary>
/// Bridges the non-generic <see cref="IEventHandler"/> and the strongly-typed <see cref="IEventHandler{T}"/>
/// by wrapping an <see cref="IEventHandler{T}"/> instance and casting the incoming payload to <typeparamref name="T"/>
/// before dispatching. Used by event bus implementations to invoke typed handlers through a uniform non-generic interface.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public class ServiceEventHandler<T>(IEventHandler<T> service) : IEventHandler
{
    public Task HandleAsync(object payload)
    {
        return service.HandleAsync((T) payload);
    }
}