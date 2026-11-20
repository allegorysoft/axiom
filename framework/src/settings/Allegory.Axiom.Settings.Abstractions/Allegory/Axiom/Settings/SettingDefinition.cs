using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;

namespace Allegory.Axiom.Settings;

[ImmutableObject(true)]
public sealed class SettingDefinition(
    string name,
    JsonElement? defaultValue = null,
    bool isPublic = false,
    bool isEncrypted = false,
    ImmutableSortedSet<string>? contributors = null)
{
    public string Name { get; } = name;
    public JsonElement? DefaultValue { get; } = defaultValue;
    public bool IsPublic { get; } = isPublic;
    public bool IsEncrypted { get; } = isEncrypted;
    public ImmutableSortedSet<string> Contributors { get; } = contributors ?? ImmutableSortedSet<string>.Empty;

    public static SettingDefinition Create<T>(
        string name,
        T value,
        bool isPublic = false,
        bool isEncrypted = false,
        ImmutableSortedSet<string>? contributors = null)
    {
        return new SettingDefinition(
            name,
            JsonSerializer.SerializeToElement(value),
            isPublic,
            isEncrypted,
            contributors);
    }

    public SettingDefinition Clone(
        JsonElement? defaultValue = null,
        bool? isPublic = null,
        bool? isEncrypted = null,
        ImmutableSortedSet<string>? contributors = null)
    {
        return new SettingDefinition(
            Name,
            defaultValue ?? DefaultValue?.Clone(),
            isPublic ?? IsPublic,
            isEncrypted ?? IsEncrypted,
            contributors ?? Contributors);
    }

    public SettingDefinition Clone<T>(
        T defaultValue,
        bool? isPublic = null,
        bool? isEncrypted = null,
        ImmutableSortedSet<string>? contributors = null)
    {
        return new SettingDefinition(
            Name,
            JsonSerializer.SerializeToElement(defaultValue),
            isPublic ?? IsPublic,
            isEncrypted ?? IsEncrypted,
            contributors ?? Contributors);
    }

    public override bool Equals(object? obj) => obj is SettingDefinition setting && setting.Name == Name;

    public override int GetHashCode() => Name.GetHashCode();
}