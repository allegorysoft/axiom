using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface IEventHandler
{
    Task HandleAsync(object payload);
}

public class ServiceEventHandler<T>(IEventHandler<T> service) : IEventHandler
{
    public Task HandleAsync(object payload)
    {
        return service.HandleAsync((T) payload);
    }
}