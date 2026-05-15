using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantPrincipalStore
{
    Task<bool> HasAccessAsync(
        string principalId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlySet<Guid>> GetTenantListAsync(
        string principalId,
        CancellationToken cancellationToken = default);
}