using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.EventBus.Distributed.Inbox;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class NullInboxStore : IInboxStore
{
    
}