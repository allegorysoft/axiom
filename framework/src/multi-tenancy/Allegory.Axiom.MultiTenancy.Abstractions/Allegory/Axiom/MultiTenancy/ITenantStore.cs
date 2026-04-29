using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantStore : ISingletonService
{
    ValueTask<TenantContext?> FindAsync(Guid id);
    ValueTask<TenantContext?> FindAsync(string name);
}