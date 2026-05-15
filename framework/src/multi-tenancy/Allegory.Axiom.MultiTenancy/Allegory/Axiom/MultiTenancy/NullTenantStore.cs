using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class NullTenantStore : ITenantStore, ISingletonService
{
    public ValueTask<TenantContext?> FindAsync(Guid id) => ValueTask.FromResult<TenantContext?>(null);
    public ValueTask<TenantContext?> FindAsync(string name) => ValueTask.FromResult<TenantContext?>(null);
}