using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Allegory.Axiom.Caching.Converters;

public sealed class FrozenDictionaryJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(FrozenDictionary<,>);
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var arguments = typeToConvert.GetGenericArguments();

        return (JsonConverter) Activator.CreateInstance(
            typeof(FrozenDictionaryJsonConverter<,>).MakeGenericType(arguments))!;
    }
}

public sealed class FrozenDictionaryJsonConverter<TKey, TValue> :
    JsonConverter<FrozenDictionary<TKey, TValue>>
    where TKey : notnull
{
    public override FrozenDictionary<TKey, TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options);

        return dictionary?.ToFrozenDictionary() ?? FrozenDictionary<TKey, TValue>.Empty;
    }

    public override void Write(
        Utf8JsonWriter writer,
        FrozenDictionary<TKey, TValue> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<IReadOnlyDictionary<TKey, TValue>>(writer, value, options);
    }
}