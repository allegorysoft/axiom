using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantStore
{
    ValueTask<TenantContext?> FindAsync(Guid id);
    ValueTask<TenantContext?> FindAsync(string name);
}