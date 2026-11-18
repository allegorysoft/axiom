using System.Collections.Generic;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionContext
{
    public Dictionary<string, SettingDefinition> Settings { get; } = new();

    public void Add(params IEnumerable<SettingDefinition> settings)
    {
        foreach (var setting in settings)
        {
            Settings.Add(setting.Name, setting);
        }
    }
}