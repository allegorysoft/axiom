using System.Collections.Frozen;
using System.ComponentModel;

namespace Allegory.Axiom.Settings.Synchronization;

[ImmutableObject(true)]
public sealed class SettingDefinitionCacheItem
{
    public FrozenDictionary<string, SettingDefinition> Settings { get; set; } = null!;
}