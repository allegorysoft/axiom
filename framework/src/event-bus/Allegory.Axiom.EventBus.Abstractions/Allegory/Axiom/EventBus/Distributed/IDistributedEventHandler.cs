using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Distributed;

/// <summary>
/// Marker interface for distributed event handlers. Used for assembly scanning to discover
/// all <see cref="IDistributedEventHandler{T}"/> implementations without inspecting generic arguments.
/// </summary>
public interface IDistributedEventHandler {}

/// <summary>
/// Defines a handler for a distributed (out-of-process) event of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public interface IDistributedEventHandler<in T> : IDistributedEventHandler
{
    Task HandleAsync(T payload);
}