using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Extensibility;

public class ExtraPropertiesJsonSerializerTests
{
    protected ExtraPropertiesJsonSerializer Serializer { get; } = new();

    [Fact]
    public void ShouldSerializeDictionary()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["name"] = "John",
            ["age"] = 42
        };

        var json = Serializer.Serialize(dictionary);

        json.ShouldContain("\"name\":\"John\"");
        json.ShouldContain("\"age\":42");
    }

    [Fact]
    public void ShouldDeserializeString()
    {
        const string json = """{"name":"John","age":42}""";

        var result = Serializer.Deserialize(json);

        result["name"]
            .ShouldBeOfType<JsonElement>()
            .GetString().ShouldBe("John");

        result["age"]
            .ShouldBeOfType<JsonElement>()
            .GetInt32().ShouldBe(42);
    }

    [Fact]
    public void ShouldReturnEmptyDictionaryWhenDeserializingNullOrWhitespace()
    {
        Serializer.Deserialize(null).ShouldBeEmpty();
        Serializer.Deserialize("").ShouldBeEmpty();
        Serializer.Deserialize("   ").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldCloneDictionary()
    {
        var original = new Dictionary<string, object>
        {
            ["name"] = "John",
            ["age"] = 42
        };

        var clone = Serializer.Clone(original);

        clone.ShouldNotBeSameAs(original);

        // Deserialized extra property values are stored as JsonElement.
        // ExtraPropertiesExtensions converts them to the requested CLR type when accessed.
        clone["name"]
            .ShouldBeOfType<JsonElement>()
            .GetString().ShouldBe("John");
        clone["age"]
            .ShouldBeOfType<JsonElement>()
            .GetInt32().ShouldBe(42);
    }

    [Fact]
    public void ShouldReturnTrueWhenComparingEqualDictionaries()
    {
        var left = new Dictionary<string, object> {["name"] = "John", ["age"] = 42};
        var right = new Dictionary<string, object> {["name"] = "John", ["age"] = 42};

        Serializer.AreEqual(left, right).ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnFalseWhenComparingDifferentDictionaries()
    {
        var left = new Dictionary<string, object> {["name"] = "John"};
        var right = new Dictionary<string, object> {["name"] = "Jane"};

        Serializer.AreEqual(left, right).ShouldBeFalse();
    }

    [Fact]
    public void ShouldReturnFalseWhenComparingNullDictionaries()
    {
        var dictionary = new Dictionary<string, object> {["name"] = "John"};

        Serializer.AreEqual(null, dictionary).ShouldBeFalse();
        Serializer.AreEqual(dictionary, null).ShouldBeFalse();
        Serializer.AreEqual(null, null).ShouldBeTrue(); // ReferenceEquals
    }

    [Fact]
    public void ShouldReturnTrueWhenComparingSameReference()
    {
        var dictionary = new Dictionary<string, object> {["name"] = "John"};

        Serializer.AreEqual(dictionary, dictionary).ShouldBeTrue();
    }

    [Fact]
    public void ShouldGetHashCodeConsistently()
    {
        var dictionary = new Dictionary<string, object> {["name"] = "John", ["age"] = 42};

        var hash1 = Serializer.GetHashCode(dictionary);
        var hash2 = Serializer.GetHashCode(dictionary);

        hash1.ShouldBe(hash2);
    }
}