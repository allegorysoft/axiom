using System.Collections.Generic;
using System.Text.Json;

namespace Allegory.Axiom.Localization;

public class LocalizationSerializer
{
    public static LocalizationSerializer Instance { get; set; } = new();

    public JsonSerializerOptions Options { get; set; } = new();

    public virtual string Serialize(IDictionary<string, string> value)
    {
        return value.Count == 0 ? string.Empty : JsonSerializer.Serialize(value, Options);
    }

    public virtual Dictionary<string, string> Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(value, Options) ?? [];
    }
}