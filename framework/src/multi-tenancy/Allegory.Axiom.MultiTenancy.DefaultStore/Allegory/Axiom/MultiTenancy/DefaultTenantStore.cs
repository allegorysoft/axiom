using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.MultiTenancy;

public class DefaultTenantStore(
    IOptions<DefaultTenantStoreOptions> options,
    ITenantNormalizer tenantNormalizer)
    : ITenantStore
{
    protected DefaultTenantStoreOptions Options { get; } = options.Value;
    protected ITenantNormalizer TenantNormalizer { get; } = tenantNormalizer;

    public virtual ValueTask<TenantContext?> FindAsync(Guid id)
    {
        return ValueTask.FromResult(
            Options.Tenants.SingleOrDefault(t => t.Id == id));
    }

    public virtual ValueTask<TenantContext?> FindAsync(string name)
    {
        name = TenantNormalizer.NormalizeName(name);
        return ValueTask.FromResult(
            Options.Tenants.SingleOrDefault(t => t.NormalizedName == name));
    }
}