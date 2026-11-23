using System.Collections.Frozen;
using System.ComponentModel;

namespace Allegory.Axiom.Setting;

[ImmutableObject(true)]
public sealed class SettingDefinitionCacheItem
{
    public FrozenDictionary<string, SettingDefinition> Definitions { get; init; } = null!;
}