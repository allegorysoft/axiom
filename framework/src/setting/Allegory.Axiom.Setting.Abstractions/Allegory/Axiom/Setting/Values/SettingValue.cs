using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Allegory.Axiom.Setting;

[ImmutableObject(true)]
public sealed class SettingValue(JsonElement value)
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = {new JsonStringEnumConverter()}
    };

    private readonly Lock _lock = new();
    private object? _object;
    private Type? _objectType;

    public JsonElement Value { get; } = value;

    public T Get<T>()
    {
        var result = Volatile.Read(ref _object);

        if (result is not null)
            return Validate<T>(result);

        lock (_lock)
        {
            result = _object;

            if (result is null)
            {
                result = Value.Deserialize<T>(DefaultOptions)
                         ?? throw new InvalidOperationException(
                             "Setting value deserialization returned null.");

                _objectType = typeof(T);

                Volatile.Write(ref _object, result);
            }
        }

        return Validate<T>(result);
    }

    private T Validate<T>(object value)
    {
        return value is T result
            ? result
            : throw new InvalidOperationException(
                $"Setting value was already deserialized as {_objectType}, " +
                $"but {typeof(T)} was requested.");
    }
}