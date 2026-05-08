using System;
using System.Collections.Generic;

namespace Allegory.Axiom.MultiTenancy;

public class DefaultTenantStoreOptions
{
    public TenantContext[] Tenants { get; set; } = [];
    public Dictionary<string, HashSet<Guid>> TenantPrincipals { get; set; } = [];
}