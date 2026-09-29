using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.DistributedLocking;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class InProcessDistributedLock : DistributedLockBase
{
}