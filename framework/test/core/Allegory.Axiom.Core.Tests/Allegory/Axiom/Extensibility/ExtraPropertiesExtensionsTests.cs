using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Extensibility;

public class ExtraPropertiesExtensionsTests
{
    [Fact]
    public void ShouldGetPropertyWhenValueIsOfRequestedType()
    {
        var immutable = new TestReadOnlyExtraProperties(new Dictionary<string, object> {{"age", 42}});
        var immutableResult = immutable.GetProperty<int>("age");
        immutableResult.ShouldBe(42);

        var mutable = new TestExtraProperties();
        mutable.ExtraProperties["age"] = 43;
        var mutableResult = mutable.GetProperty<int>("age");
        mutableResult.ShouldBe(43);
    }

    [Fact]
    public void ShouldConvertJsonElementWhenGettingProperty()
    {
        var immutable = new TestReadOnlyExtraProperties(
            new Dictionary<string, object> {{"age", JsonSerializer.SerializeToElement(42)}});
        var immutableResult = immutable.GetProperty<int>("age");
        immutableResult.ShouldBe(42);

        var mutable = new TestExtraProperties();
        mutable.ExtraProperties["age"] = JsonSerializer.SerializeToElement(43);
        var mutableResult = mutable.GetProperty<int>("age");
        mutableResult.ShouldBe(43);
    }

    [Fact]
    public void ShouldConvertUsingChangeTypeWhenGettingProperty()
    {
        var immutable = new TestReadOnlyExtraProperties(new Dictionary<string, object> {{"age", "42"}});
        var immutableResult = immutable.GetProperty<int>("age");
        immutableResult.ShouldBe(42);

        var mutable = new TestExtraProperties();
        mutable.ExtraProperties["age"] = "43";
        var mutableResult = mutable.GetProperty<int>("age");
        mutableResult.ShouldBe(43);
    }

    [Fact]
    public void ShouldUpdateStoredValueAfterConversionForMutableExtraProperties()
    {
        var properties = new TestExtraProperties();
        var json = JsonSerializer.SerializeToElement(42);
        properties.ExtraProperties["age"] = json;

        var result = properties.GetProperty<int>("age");

        result.ShouldBe(42);
        properties.ExtraProperties["age"].ShouldBe(42); // stored value should be updated
        properties.ExtraProperties["age"].ShouldBeOfType<int>();
        properties.ExtraProperties["age"].ShouldNotBeOfType<JsonElement>();
    }

    [Fact]
    public void ShouldThrowWhenConvertIsFalseAndTypeMismatch()
    {
        var immutable = new TestReadOnlyExtraProperties(new Dictionary<string, object> {{"age", "42"}});
        Should.Throw<ArgumentException>(() => immutable.GetProperty<int>("age", convert: false));

        var mutable = new TestExtraProperties();
        mutable.ExtraProperties["age"] = "43";
        Should.Throw<ArgumentException>(() => mutable.GetProperty<int>("age", convert: false));
    }

    [Fact]
    public void ShouldThrowWhenKeyNotFound()
    {
        var immutable = new TestReadOnlyExtraProperties(ImmutableDictionary<string, object>.Empty);
        Should.Throw<KeyNotFoundException>(() => immutable.GetProperty<int>("missing"));

        var mutable = new TestExtraProperties();
        Should.Throw<KeyNotFoundException>(() => mutable.GetProperty<int>("missing"));
    }

    [Fact]
    public void ShouldThrowWhenConversionFail()
    {
        var immutable = new TestReadOnlyExtraProperties(new Dictionary<string, object> {{"value", new object()}});
        Should.Throw<InvalidCastException>(() => immutable.GetProperty<string>("value"));

        var mutable = new TestExtraProperties();
        mutable.ExtraProperties["value"] = new object();
        Should.Throw<InvalidCastException>(() => mutable.GetProperty<string>("value"));
    }

    [Fact]
    public void ShouldReturnDefaultWhenTryGetPropertyAndKeyNotFound()
    {
        var immutable = new TestReadOnlyExtraProperties(ImmutableDictionary<string, object>.Empty);
        var immutableResult = immutable.TryGetProperty("missing", 99);
        immutableResult.ShouldBe(99);

        var mutable = new TestExtraProperties();
        var mutableResult = mutable.TryGetProperty("missing", 100);
        mutableResult.ShouldBe(100);
    }

    [Fact]
    public void ShouldReturnValueWhenTryGetPropertyAndKeyFound()
    {
        var immutable = new TestReadOnlyExtraProperties(new Dictionary<string, object> {{"age", "42"}});
        var immutableResult = immutable.TryGetProperty<int>("age");
        immutableResult.ShouldBe(42);

        var mutable = new TestExtraProperties();
        mutable.ExtraProperties["age"] = 43;
        var mutableResult = mutable.TryGetProperty<int>("age");
        mutableResult.ShouldBe(43);
    }

    [Fact]
    public void ShouldAddPropertyWhenGetOrAddPropertyWithFactory()
    {
        var properties = new TestExtraProperties();

        var result = properties.GetOrAddProperty("name", () => "John");

        result.ShouldBe("John");
        properties.ExtraProperties["name"].ShouldBe("John");
    }

    [Fact]
    public void ShouldReturnExistingPropertyWhenGetOrAddPropertyWithFactory()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["name"] = "Jane";

        var result = properties.GetOrAddProperty("name", () => "John");

        result.ShouldBe("Jane");
        properties.ExtraProperties["name"].ShouldBe("Jane");
    }

    [Fact]
    public void ShouldAddPropertyWhenGetOrAddPropertyWithValue()
    {
        var properties = new TestExtraProperties();

        var result = properties.GetOrAddProperty("name", "John");

        result.ShouldBe("John");
        properties.ExtraProperties["name"].ShouldBe("John");
    }

    [Fact]
    public void ShouldReturnExistingPropertyWhenGetOrAddPropertyWithValue()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["name"] = "Jane";

        var result = properties.GetOrAddProperty("name", "John");

        result.ShouldBe("Jane");
        properties.ExtraProperties["name"].ShouldBe("Jane");
    }

    [Fact]
    public void ShouldSetProperty()
    {
        var properties = new TestExtraProperties();

        properties.SetProperty("name", "John");

        properties.ExtraProperties["name"].ShouldBe("John");
    }

    [Fact]
    public void ShouldRemovePropertyWhenSettingNull()
    {
        var properties = new TestExtraProperties();
        properties.ExtraProperties["name"] = "John";

        properties.SetProperty("name", null);

        properties.ExtraProperties.ContainsKey("name").ShouldBeFalse();
    }
}

file class TestReadOnlyExtraProperties(IReadOnlyDictionary<string, object> properties) : IReadOnlyExtraProperties
{
    public IReadOnlyDictionary<string, object> ExtraProperties { get; } = properties;
}

file class TestExtraProperties : IExtraProperties
{
    public IDictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
}