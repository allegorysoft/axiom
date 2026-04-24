using System;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantOwned
{
    Guid? TenantId { get; }
}