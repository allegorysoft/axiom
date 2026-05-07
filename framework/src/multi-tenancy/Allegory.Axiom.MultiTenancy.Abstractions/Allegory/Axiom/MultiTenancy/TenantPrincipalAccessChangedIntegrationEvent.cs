using System;

namespace Allegory.Axiom.MultiTenancy;

public class TenantPrincipalAccessChangedIntegrationEvent
{
    public required string PrincipalId { get; set; }
    public required Guid TenantId { get; set; }
    public bool HasAccess { get; set; }
}