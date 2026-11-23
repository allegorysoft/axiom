using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.EventBus.Distributed;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class NullOutboxStore : IOutboxStore;