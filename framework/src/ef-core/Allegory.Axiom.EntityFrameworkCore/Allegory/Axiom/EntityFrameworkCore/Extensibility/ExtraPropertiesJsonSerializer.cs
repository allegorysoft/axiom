using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Allegory.Axiom.EntityFrameworkCore.Extensibility;

internal static class ExtraPropertiesJsonSerializer
{
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.General);

    internal static string Serialize(IDictionary<string, object?> value)
    {
        return JsonSerializer.Serialize(value, Options);
    }

    internal static Dictionary<string, object?> Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(value, Options) ?? [];
    }

    internal static Dictionary<string, object?> Clone(IDictionary<string, object?> value)
    {
        return Deserialize(Serialize(value));
    }

    internal static bool AreEqual(
        IDictionary<string, object?>? left,
        IDictionary<string, object?>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var rightValue))
            {
                return false;
            }

            var leftJson = JsonSerializer.Serialize(pair.Value, Options);
            var rightJson = JsonSerializer.Serialize(rightValue, Options);

            if (!string.Equals(leftJson, rightJson, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    internal static int GetHashCode(IDictionary<string, object?> value)
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