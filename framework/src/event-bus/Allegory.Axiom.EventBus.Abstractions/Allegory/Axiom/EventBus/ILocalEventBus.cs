using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface ILocalEventBus
{
    IDisposable Subscribe(string eventName, Func<object, Task> handler);
    IDisposable Subscribe<T>(Func<T, Task> handler) where T : notnull;
    Task PublishAsync(string eventName, object payload, bool onUnitOfWorkComplete = true);
    Task PublishAsync<T>(T payload, bool onUnitOfWorkComplete = true) where T : notnull;
}