using System;

namespace Allegory.Axiom.MultiTenancy;

public sealed record TenantContext(
    Guid Id,
    string Name,
    string NormalizedName);