using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantProvider(
    IEnumerable<ICurrentTenantResolver> resolvers,
    ITenantStore store,
    ITenantNormalizer normalizer,
    ICurrentTenantChecker checker)
    : ICurrentTenantProvider
{
    protected IEnumerable<ICurrentTenantResolver> Resolvers { get; } = resolvers;
    protected ITenantStore Store { get; } = store;
    public ITenantNormalizer Normalizer { get; } = normalizer;
    public ICurrentTenantChecker Checker { get; } = checker;

    public async ValueTask<TenantContext?> TryGetAsync()
    {
        string? tenant = null;

        foreach (var provider in Resolvers)
        {
            tenant = await provider.TryGetAsync();
            if (!string.IsNullOrEmpty(tenant))
            {
                break;
            }
        }

        if (string.IsNullOrEmpty(tenant))
        {
            return null;
        }

        var current = await TryGetAsync(tenant);

        if (current != null)
        {
            await Checker.CheckAsync(current);
        }

        return current;
    }

    protected virtual ValueTask<TenantContext?> TryGetAsync(string tenant)
    {
        return Guid.TryParse(tenant, out var id)
            ? Store.FindAsync(id)
            : Store.FindAsync(Normalizer.NormalizeName(tenant));
    }
}