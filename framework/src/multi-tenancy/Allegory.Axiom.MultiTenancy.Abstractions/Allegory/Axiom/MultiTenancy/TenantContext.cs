using System;
using System.Collections.Generic;
using System.ComponentModel;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.MultiTenancy;

[ImmutableObject(true)]
public sealed class TenantContext(
    Guid id,
    string name,
    string normalizedName,
    IReadOnlyDictionary<string, object?>? extraProperties = null) 
    : IReadOnlyExtraProperties
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string NormalizedName { get; } = normalizedName;
    public IReadOnlyDictionary<string, object?> ExtraProperties { get; } = extraProperties ?? new Dictionary<string, object?>();
}