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
        var identifier = await TryGetIdentifierAsync();

        if (string.IsNullOrEmpty(identifier))
        {
            return null;
        }

        var tenant = await GetTenantAsync(identifier);

        await Checker.CheckAsync(tenant);

        return tenant;
    }

    protected virtual async ValueTask<string?> TryGetIdentifierAsync()
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

        return identifier;
    }

    protected virtual async ValueTask<TenantContext> GetTenantAsync(string identifier)
    {
        var tenant = Guid.TryParse(identifier, out var id)
            ? await Store.FindAsync(id)
            : await Store.FindAsync(identifier);

        if (tenant == null)
        {
            throw new Exception($"Tenant ({identifier}) couldn't be found.");
        }

        //Check tenant.IsActive

        return tenant;
    }
}