using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.EventBus.Distributed.Outbox;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class NullOutboxStore : IOutboxStore {}