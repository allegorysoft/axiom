using System;
using System.Collections.Generic;

namespace Allegory.Axiom.Extensibility;

public static class ExtraPropertiesExtensions
{
    extension(IReadOnlyExtraProperties entity)
    {
        public bool HasProperty(string name)
        {
            return entity.ExtraProperties.ContainsKey(name);
        }

        public object? GetProperty(string name, object? defaultValue = null)
        {
            return entity.ExtraProperties.GetValueOrDefault(name, defaultValue);
        }

        public T? GetProperty<T>(string name, T? defaultValue = default, bool convert = true)
        {
            if (entity.ExtraProperties.TryGetValue(name, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                if (convert)
                {
                    return Convert.ChangeType(value, typeof(T)) is T convertedValue ? convertedValue : defaultValue;
                }
            }

            return defaultValue;
        }
    }

    extension(IExtraProperties entity)
    {
        public bool HasProperty(string name)
        {
            return entity.ExtraProperties.ContainsKey(name);
        }

        public object? GetProperty(string name, object? defaultValue = null)
        {
            return entity.ExtraProperties.TryGetValue(name, out var value) ? value : defaultValue;
        }

        public T? GetProperty<T>(string name, T? defaultValue = default, bool convert = true)
        {
            if (entity.ExtraProperties.TryGetValue(name, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }

                if (convert)
                {
                    return Convert.ChangeType(value, typeof(T)) is T convertedValue ? convertedValue : defaultValue;
                }
            }

            return defaultValue;
        }

        public void SetProperty(string name, object? value)
        {
            entity.ExtraProperties[name] = value;
        }

        public bool RemoveProperty(string name)
        {
            return entity.ExtraProperties.Remove(name);
        }
    }
}