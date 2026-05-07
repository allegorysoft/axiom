using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantPrincipalStore : ISingletonService
{
    Task<bool> HasAccessAsync(
        string principalId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlySet<Guid>> GetTenantListAsync(
        string principalId,
        CancellationToken cancellationToken = default);
}