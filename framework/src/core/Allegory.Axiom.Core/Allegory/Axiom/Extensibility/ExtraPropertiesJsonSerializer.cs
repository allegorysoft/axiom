using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allegory.Axiom.EntityFrameworkCore.Extensibility;

public class ExtraPropertiesJsonSerializer
{
    public static ExtraPropertiesJsonSerializer Instance { get; set; } = new();

    public JsonSerializerOptions Options { get; set; } = new(JsonSerializerDefaults.General);

    public virtual string Serialize(IDictionary<string, object> value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    public virtual Dictionary<string, object> Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Dictionary<string, object>>(value, Options) ?? [];
    }

    public virtual Dictionary<string, object> Clone(IDictionary<string, object> value)
    {
        return Deserialize(Serialize(value));
    }

    public virtual bool AreEqual(IDictionary<string, object>? left, IDictionary<string, object>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        return JsonElement.DeepEquals(
            JsonSerializer.SerializeToElement(left, Options),
            JsonSerializer.SerializeToElement(right, Options));
    }

    public virtual int GetHashCode(IDictionary<string, object> value)
    {
        var hash = new HashCode();

        foreach (var pair in value.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            hash.Add(pair.Key, StringComparer.Ordinal);

            hash.Add(JsonSerializer.Serialize(pair.Value, Options), StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}