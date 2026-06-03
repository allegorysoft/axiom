using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface ILocalEventHandler<in T>
{
    Task HandleAsync(T payload);
}