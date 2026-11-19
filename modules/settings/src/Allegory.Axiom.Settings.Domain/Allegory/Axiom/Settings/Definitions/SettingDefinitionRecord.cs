using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionRecord : AggregateRoot<Guid>
{
    public const int MaxNameLength = 128;
    public const int MaxDefaultValue = 1_000;

    public string Name { get; protected set; } = null!;
    public string? DefaultValue { get; protected set; }
    public bool IsPublic { get; set; }
    public bool IsEncrypted { get; set; }
    public ImmutableHashSet<string> Contributors { get; set; } = ImmutableHashSet<string>.Empty;

    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength);

        Name = name;
    }

    public void SetDefaultValue(JsonElement? defaultValue)
    {
        if (defaultValue == null)
        {
            DefaultValue = null;
            return;
        }

        DefaultValue = JsonSerializer.Serialize(defaultValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(DefaultValue.Length, MaxDefaultValue);
    }
}