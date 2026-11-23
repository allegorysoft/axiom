using System.Collections.Frozen;
using System.ComponentModel;

namespace Allegory.Axiom.Settings;

[ImmutableObject(true)]
public sealed class SettingDefinitionsCacheItem
{
    public FrozenDictionary<string, SettingDefinition> Settings { get; init; } = null!;
}