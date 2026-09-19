using System;
using System.Collections.Generic;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Extensibility;

public class ExtraPropertiesManagerTests
{
    protected ExtraPropertiesManager Manager { get; } = new();

    // GetProperty

    [Fact]
    public void ShouldGetPropertyWhenValueIsOfRequestedType()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["age"] = 43;

        var result = Manager.GetProperty<int>(properties, "age");

        result.ShouldBe(43);
    }

    [Fact]
    public void ShouldConvertJsonElementWhenGettingProperty()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["age"] = JsonSerializer.SerializeToElement(43);

        var result = Manager.GetProperty<int>(properties, "age");

        result.ShouldBe(43);
    }

    [Fact]
    public void ShouldConvertUsingChangeTypeWhenGettingProperty()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["age"] = "43";

        var result = Manager.GetProperty<int>(properties, "age");

        result.ShouldBe(43);
    }

    [Fact]
    public void ShouldUpdateStoredValueAfterConversion()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["age"] = JsonSerializer.SerializeToElement(42);

        var result = Manager.GetProperty<int>(properties, "age");

        result.ShouldBe(42);
        properties.ExtraProperties["age"].ShouldBe(42); // stored value should be updated
        properties.ExtraProperties["age"].ShouldBeOfType<int>();
        properties.ExtraProperties["age"].ShouldNotBeOfType<JsonElement>();
    }

    [Fact]
    public void ShouldThrowWhenConvertIsFalseAndTypeMismatch()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["age"] = "43";

        Should.Throw<ArgumentException>(() => Manager.GetProperty<int>(properties, "age", convert: false));
    }

    [Fact]
    public void ShouldThrowWhenKeyNotFound()
    {
        var properties = new TestExtraProperties();

        Should.Throw<KeyNotFoundException>(() => Manager.GetProperty<int>(properties, "missing"));
    }

    [Fact]
    public void ShouldThrowWhenConversionFail()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["value"] = new object();

        Should.Throw<InvalidCastException>(() => Manager.GetProperty<string>(properties, "value"));
    }

    // TryGetProperty

    [Fact]
    public void ShouldReturnDefaultWhenTryGetPropertyAndKeyNotFound()
    {
        var properties = new TestExtraProperties();

        var mutableResult = Manager.TryGetProperty(properties, "missing", 100);

        mutableResult.ShouldBe(100);
    }

    [Fact]
    public void ShouldReturnValueWhenTryGetPropertyAndKeyFound()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["age"] = 43;

        var mutableResult = Manager.TryGetProperty<int>(properties, "age");

        mutableResult.ShouldBe(43);
    }

    // GetOrAddProperty factory

    [Fact]
    public void ShouldAddPropertyWhenGetOrAddPropertyWithFactory()
    {
        var properties = new TestExtraProperties();

        var result = Manager.GetOrAddProperty(properties, "name", () => "John");

        result.ShouldBe("John");
        properties.ExtraProperties["name"].ShouldBe("John");
    }

    [Fact]
    public void ShouldReturnExistingPropertyWhenGetOrAddPropertyWithFactory()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["name"] = "Jane";

        var result = Manager.GetOrAddProperty(properties, "name", () => "John");

        result.ShouldBe("Jane");
        properties.ExtraProperties["name"].ShouldBe("Jane");
    }

    // GetOrAddProperty instance

    [Fact]
    public void ShouldAddPropertyWhenGetOrAddPropertyWithValue()
    {
        var properties = new TestExtraProperties();

        var result = Manager.GetOrAddProperty(properties, "name", "John");

        result.ShouldBe("John");
        properties.ExtraProperties["name"].ShouldBe("John");
    }

    [Fact]
    public void ShouldReturnExistingPropertyWhenGetOrAddPropertyWithValue()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["name"] = "Jane";

        var result = Manager.GetOrAddProperty(properties, "name", "John");

        result.ShouldBe("Jane");
        properties.ExtraProperties["name"].ShouldBe("Jane");
    }

    // SetProperty

    [Fact]
    public void ShouldSetProperty()
    {
        var properties = new TestExtraProperties();

        Manager.SetProperty(properties, "name", "John");

        properties.ExtraProperties["name"].ShouldBe("John");
    }

    [Fact]
    public void ShouldRemovePropertyWhenSettingNull()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["name"] = "John";

        Manager.SetProperty(properties, "name", null);

        properties.ExtraProperties.ContainsKey("name").ShouldBeFalse();
    }

    // Serialization

    [Fact]
    public void ShouldSerializeDictionary()
    {
        var dictionary = new Dictionary<string, object>
        {
            ["name"] = "John",
            ["age"] = 42
        };

        var json = Manager.Serialize(dictionary);

        json.ShouldContain("\"name\":\"John\"");
        json.ShouldContain("\"age\":42");
    }

    [Fact]
    public void ShouldDeserializeString()
    {
        const string json = """{"name":"John","age":42}""";

        var result = Manager.Deserialize(json);

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
        Manager.Deserialize(null).ShouldBeEmpty();
        Manager.Deserialize("").ShouldBeEmpty();
        Manager.Deserialize("   ").ShouldBeEmpty();
    }
}

file class TestExtraProperties : IExtraProperties
{
    public IDictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
}