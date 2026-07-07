using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus.Local;

/// <summary>
/// Marker interface for local event handlers. Used for assembly scanning to discover
/// all <see cref="ILocalEventHandler{T}"/> implementations without inspecting generic arguments.
/// </summary>
public interface ILocalEventHandler {}

/// <summary>
/// Defines a handler for a local (in-process) event of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public interface ILocalEventHandler<in T> : ILocalEventHandler
{
    Task HandleAsync(T payload);
}