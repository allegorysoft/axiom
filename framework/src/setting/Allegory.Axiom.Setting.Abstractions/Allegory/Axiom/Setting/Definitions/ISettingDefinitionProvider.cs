using System.Collections.Frozen;

namespace Allegory.Axiom.Setting;

public interface ISettingDefinitionProvider
{
    FrozenDictionary<string, SettingDefinition> Settings { get; }
}