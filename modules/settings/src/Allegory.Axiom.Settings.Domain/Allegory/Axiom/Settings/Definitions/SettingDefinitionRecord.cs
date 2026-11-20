using System;
using System.Collections.Immutable;
using System.Text.Json;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionRecord : AggregateRoot<Guid>
{
    public const int MaxNameLength = 128;
    public const int MaxDefaultValue = 1_000;

    protected SettingDefinitionRecord() { }

    public SettingDefinitionRecord(
        string name,
        JsonElement? defaultValue = null,
        bool isPublic = false,
        bool isEncrypted = false,
        ImmutableSortedSet<string>? contributors = null)
    {
        SetName(name);
        SetDefaultValue(defaultValue);

        IsPublic = isPublic;
        IsEncrypted = isEncrypted;
        Contributors = contributors ?? ImmutableSortedSet<string>.Empty;
    }

    public string Name { get; protected set; } = null!;
    public string? DefaultValue { get; protected set; }
    public bool IsPublic { get; set; }
    public bool IsEncrypted { get; set; }
    public ImmutableSortedSet<string> Contributors { get; set; } = null!;

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