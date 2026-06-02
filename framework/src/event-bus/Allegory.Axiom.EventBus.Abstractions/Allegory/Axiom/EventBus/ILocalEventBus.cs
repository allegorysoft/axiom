using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface ILocalEventBus
{
    IDisposable Subscribe(string eventName, Func<object, Task> handler);
    Task PublishAsync(string eventName, object payload, bool onUnitOfWorkComplete = true);
    void Unsubscribe(string eventName, Func<object, Task> handler);

    // Generic versions
    //Task PublishAsync<TEvent>(TEvent payload, bool onUnitOfWorkComplete = true);
    //IDisposable Subscribe<TEvent>(Func<TEvent, Task> onEvent);
    //Unsubscribe
}