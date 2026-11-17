using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Allegory.Axiom.Caching.Converters;

public sealed class FrozenDictionaryJsonConverter<TKey, TValue> :
    JsonConverter<FrozenDictionary<TKey, TValue>>
    where TKey : notnull
{
    public override FrozenDictionary<TKey, TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var dictionary = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options)!;

        return dictionary.ToFrozenDictionary();
    }

    public override void Write(
        Utf8JsonWriter writer,
        FrozenDictionary<TKey, TValue> value,
        JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToDictionary(), options);
    }
}