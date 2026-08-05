using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.MultiTenancy;

[ImmutableObject(true)]
public sealed class TenantContext(
    Guid id,
    string name,
    string normalizedName,
    IReadOnlyDictionary<string, string>? connectionStrings = null,
    IReadOnlyDictionary<string, object?>? extraProperties = null,
    bool isActive = true)
    : IReadOnlyExtraProperties
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string NormalizedName { get; } = normalizedName;
    public bool IsActive { get; } = isActive;

    public IReadOnlyDictionary<string, string> ConnectionStrings { get; } =
        connectionStrings ?? FrozenDictionary<string, string>.Empty;

    public IReadOnlyDictionary<string, object?> ExtraProperties { get; } =
        extraProperties ?? FrozenDictionary<string, object?>.Empty;
}