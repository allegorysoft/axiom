using System.Collections.Frozen;

namespace Allegory.Axiom.Settings;

public interface ISettingDefinitionProvider
{
    FrozenDictionary<string, SettingDefinition> Settings { get; }
}