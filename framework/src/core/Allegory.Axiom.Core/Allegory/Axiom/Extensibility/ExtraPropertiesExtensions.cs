using System;
using System.Text.Json;

namespace Allegory.Axiom.Extensibility;

public static class ExtraPropertiesExtensions
{
    extension(IReadOnlyExtraProperties entity)
    {
        public T GetProperty<T>(string name, bool convert = true)
        {
            var value = entity.ExtraProperties[name];

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
                value = jsonElement.Deserialize<T>();
            }
            else
            {
                value = (T) Convert.ChangeType(value, typeof(T));
            }

            return (T) (value ?? throw new InvalidCastException($"Conversion of property '{name}' returned null."));
        }

        public T? TryGetProperty<T>(string name, T? defaultValue = default, bool convert = true)
        {
            return entity.ExtraProperties.ContainsKey(name) ? entity.GetProperty<T>(name, convert) : defaultValue;
        }
    }

    extension(IExtraProperties entity)
    {
        public T GetProperty<T>(string name, bool convert = true)
        {
            var value = entity.ExtraProperties[name];

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
                value = jsonElement.Deserialize<T>();
            }
            else
            {
                value = (T) Convert.ChangeType(value, typeof(T));
            }

            entity.ExtraProperties[name] =
                value ?? throw new InvalidCastException($"Conversion of property '{name}' returned null.");

            return (T) value;
        }

        public T? TryGetProperty<T>(string name, T? defaultValue = default, bool convert = true)
        {
            return entity.ExtraProperties.ContainsKey(name) ? entity.GetProperty<T>(name, convert) : defaultValue;
        }

        public T GetOrAddProperty<T>(string name, Func<T> factory) where T : notnull
        {
            if (entity.ExtraProperties.ContainsKey(name))
            {
                return entity.GetProperty<T>(name);
            }

            var property = factory();
            entity.ExtraProperties[name] = property;
            return property;
        }

        public T GetOrAddProperty<T>(string name, T value) where T : notnull
        {
            if (entity.ExtraProperties.ContainsKey(name))
            {
                return entity.GetProperty<T>(name);
            }

            entity.ExtraProperties[name] = value;
            return value;
        }

        public void SetProperty(string name, object? value)
        {
            if (value == null)
            {
                entity.ExtraProperties.Remove(name);
                return;
            }

            entity.ExtraProperties[name] = value;
        }
    }
}