using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.MultiTenancy;

public class DefaultTenantPrincipalStore(IOptions<DefaultTenantStoreOptions> options) : ITenantPrincipalStore, ISingletonService
{
    public DefaultTenantStoreOptions Options { get; } = options.Value;

    public async Task<bool> HasAccessAsync(
        string principalId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var tenants = await GetTenantListAsync(principalId, cancellationToken);
        return tenants.Contains(tenantId);
    }

    public ValueTask<IReadOnlySet<Guid>> GetTenantListAsync(
        string principalId,
        CancellationToken cancellationToken = default)
    {
        return Options.TenantPrincipals.TryGetValue(principalId, out var tenants)
            ? ValueTask.FromResult<IReadOnlySet<Guid>>(tenants)
            : ValueTask.FromResult<IReadOnlySet<Guid>>(ImmutableHashSet<Guid>.Empty);
    }
}