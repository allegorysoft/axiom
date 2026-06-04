using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

/// <summary>
/// Defines a handler for an event of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public interface IEventHandler<in T>
{
    Task HandleAsync(T payload);
}

/// <summary>
/// Marker interface for local event handlers. Used for assembly scanning to discover
/// all <see cref="ILocalEventHandler{T}"/> implementations without inspecting generic arguments.
/// </summary>
public interface ILocalEventHandler {}

/// <summary>
/// Defines a handler for a local (in-process) event of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public interface ILocalEventHandler<in T> : IEventHandler<T>, ILocalEventHandler {}

/// <summary>
/// Marker interface for distributed event handlers. Used for assembly scanning to discover
/// all <see cref="IDistributedEventHandler{T}"/> implementations without inspecting generic arguments.
/// </summary>
public interface IDistributedEventHandler {}

/// <summary>
/// Defines a handler for a distributed (out-of-process) event of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public interface IDistributedEventHandler<in T> : IEventHandler<T>, IDistributedEventHandler {}