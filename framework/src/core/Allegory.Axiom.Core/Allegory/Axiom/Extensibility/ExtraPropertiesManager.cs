using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Allegory.Axiom.Extensibility;

public class ExtraPropertiesManager
{
    public static ExtraPropertiesManager Instance { get; set; } = new();

    public JsonSerializerOptions Options { get; set; } = new(JsonSerializerDefaults.Web);

    public virtual T GetProperty<T>(IExtraProperties source, string name, bool convert = true)
    {
        var value = source.ExtraProperties[name];

        if (value is T typedValue)
        {
            return typedValue;
        }

        if (!convert)
        {
            throw new ArgumentException(
                $"Cannot get property '{name}': stored value is of type '{value.GetType().FullName}' " +
                $"but requested type is '{typeof(T).FullName}'. Conversion is disabled (convert = false).");
        }

        if (value is JsonElement jsonElement)
        {
            value = jsonElement.Deserialize<T>(Options);
        }
        else
        {
            value = (T) Convert.ChangeType(value, typeof(T));
        }

        source.ExtraProperties[name] =
            value ?? throw new InvalidCastException($"Conversion of property '{name}' returned null.");

        return (T) value;
    }

    public virtual T? TryGetProperty<T>(
        IExtraProperties source,
        string name,
        T? defaultValue = default,
        bool convert = true)
    {
        return source.ExtraProperties.ContainsKey(name) ? source.GetProperty<T>(name, convert) : defaultValue;
    }

    public virtual T GetOrAddProperty<T>(IExtraProperties source, string name, Func<T> factory) where T : notnull
    {
        if (source.ExtraProperties.ContainsKey(name))
        {
            return source.GetProperty<T>(name);
        }

        var property = factory();
        source.ExtraProperties[name] = property;
        return property;
    }

    public virtual T GetOrAddProperty<T>(IExtraProperties source, string name, T value) where T : notnull
    {
        if (source.ExtraProperties.ContainsKey(name))
        {
            return source.GetProperty<T>(name);
        }

        source.ExtraProperties[name] = value;
        return value;
    }

    public virtual void SetProperty(IExtraProperties source, string name, object? value)
    {
        if (value == null)
        {
            source.ExtraProperties.Remove(name);
            return;
        }

        source.ExtraProperties[name] = value;
    }

    public virtual string Serialize(IDictionary<string, object> value)
    {
        return value.Count == 0 ? string.Empty : JsonSerializer.Serialize(value, Options);
    }

    public virtual Dictionary<string, object> Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Dictionary<string, object>>(value, Options) ?? [];
    }
}