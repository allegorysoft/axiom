using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantProvider(
    IEnumerable<ICurrentTenantIdentifierProvider> providers,
    ITenantStore store,
    ICurrentTenantChecker checker)
    : ICurrentTenantProvider
{
    protected IEnumerable<ICurrentTenantIdentifierProvider> Providers { get; } = providers;
    protected ITenantStore Store { get; } = store;
    protected ICurrentTenantChecker Checker { get; } = checker;

    public async ValueTask<TenantContext?> TryGetAsync()
    {
        string? identifier = null;

        foreach (var provider in Providers)
        {
            identifier = await provider.TryGetAsync();
            if (!string.IsNullOrEmpty(identifier))
            {
                break;
            }
        }

        if (string.IsNullOrEmpty(identifier))
        {
            return null;
        }

        var tenant = await TryGetAsync(identifier);

        if (tenant == null)
        {
            throw new Exception($"Tenant ({identifier}) couldn't be found.");
        }
        
        //Check tenant.IsActive

        await Checker.CheckAsync(tenant);

        return tenant;
    }

    protected virtual ValueTask<TenantContext?> TryGetAsync(string identifier)
    {
        return Guid.TryParse(identifier, out var id)
            ? Store.FindAsync(id)
            : Store.FindAsync(identifier);
    }
}