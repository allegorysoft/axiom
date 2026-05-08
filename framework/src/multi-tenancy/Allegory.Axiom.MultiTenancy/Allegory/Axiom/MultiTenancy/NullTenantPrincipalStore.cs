using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class NullTenantPrincipalStore : ITenantPrincipalStore
{
    public Task<bool> HasAccessAsync(
        string principalId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public ValueTask<IReadOnlySet<Guid>> GetTenantListAsync(
        string principalId,
        CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IReadOnlySet<Guid>>(ImmutableHashSet<Guid>.Empty);
}