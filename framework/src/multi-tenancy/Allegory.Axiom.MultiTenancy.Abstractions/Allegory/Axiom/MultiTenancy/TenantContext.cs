using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;

namespace Allegory.Axiom.MultiTenancy;

[ImmutableObject(true)]
public sealed class TenantContext(
    Guid id,
    string name,
    string normalizedName,
    IReadOnlyDictionary<string, string>? connectionStrings = null,
    bool isActive = true)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public string NormalizedName { get; } = normalizedName;
    public bool IsActive { get; } = isActive;

    public IReadOnlyDictionary<string, string> ConnectionStrings { get; } =
        connectionStrings ?? FrozenDictionary<string, string>.Empty;
}