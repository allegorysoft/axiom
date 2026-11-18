using System.Text.Json;

namespace Allegory.Axiom.Settings;

public class SettingDefinition
{
    public string Name { get; set; } = null!;
    public JsonElement? DefaultValue { get; set; }
    public bool IsEncrypted { get; set; }

    public static SettingDefinition Create<T>(T value)
    {
        return new SettingDefinition
        {
            DefaultValue = JsonSerializer.SerializeToElement(value)
        };
    }
}