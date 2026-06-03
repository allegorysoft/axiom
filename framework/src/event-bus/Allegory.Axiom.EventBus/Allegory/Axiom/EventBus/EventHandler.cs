using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IEventHandler
{
    Task HandleAsync(object payload);
}

public class DelegateEventHandler(Func<object, Task> handler) : IEventHandler
{
    public Task HandleAsync(object payload)
    {
        return handler(payload);
    }
}

public class DelegateEventHandler<T>(Func<T, Task> handler) : IEventHandler
{
    public Task HandleAsync(object payload)
    {
        return handler((T) payload);
    }
}

public class ServiceEventHandler<T>(IEventHandler<T> service) : IEventHandler
{
    public Task HandleAsync(object payload)
    {
        return service.HandleAsync((T) payload);
    }
}