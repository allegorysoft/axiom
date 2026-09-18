using System;

namespace Allegory.Axiom.Extensibility;

public static class ExtraPropertiesExtensions
{
    extension(IExtraProperties source)
    {
        public T GetProperty<T>(string name, bool convert = true) =>
            ExtraPropertiesManager.Instance.GetProperty<T>(source, name, convert);

        public T? TryGetProperty<T>(string name, T? defaultValue = default, bool convert = true) =>
            ExtraPropertiesManager.Instance.TryGetProperty(source, name, defaultValue, convert);

        public T GetOrAddProperty<T>(string name, Func<T> factory) where T : notnull =>
            ExtraPropertiesManager.Instance.GetOrAddProperty(source, name, factory);

        public T GetOrAddProperty<T>(string name, T value) where T : notnull =>
            ExtraPropertiesManager.Instance.GetOrAddProperty(source, name, value);

        public void SetProperty(string name, object? value) =>
            ExtraPropertiesManager.Instance.SetProperty(source, name, value);
    }
}