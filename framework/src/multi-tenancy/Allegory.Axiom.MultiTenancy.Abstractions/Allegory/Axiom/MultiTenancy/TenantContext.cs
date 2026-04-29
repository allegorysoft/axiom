using System;
using System.Collections.Generic;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.MultiTenancy;

public class TenantContext(Guid id, string name, string normalizedName) : IReadOnlyExtraProperties
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string NormalizedName { get; } = normalizedName;
    public IReadOnlyDictionary<string, object?> ExtraProperties { get; } = new Dictionary<string, object?>();
}