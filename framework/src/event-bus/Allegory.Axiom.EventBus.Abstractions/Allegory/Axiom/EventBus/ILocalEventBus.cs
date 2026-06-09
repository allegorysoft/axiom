using System.Threading.Tasks;

namespace Allegory.Axiom.EventBus;

public interface ILocalEventBus
{
    Task PublishAsync<T>(
        T payload,
        LocalMessagePublishMode publishMode = LocalMessagePublishMode.OnUnitOfWorkComplete)
        where T : notnull;
}