using Shouldly;
using Xunit;

namespace Allegory.Axiom.MultiTenancy;

public class TenantNormalizerTests
{
    protected TenantNormalizer TenantNormalizer { get; } = new();

    [Fact]
    public void ShouldNormalizeLowercaseNameToUppercase()
    {
        var result = TenantNormalizer.NormalizeName("tenant");

        result.ShouldBe("TENANT");
    }

    [Fact]
    public void ShouldNormalizeMixedCaseNameToUppercase()
    {
        var result = TenantNormalizer.NormalizeName("MyTenant");

        result.ShouldBe("MYTENANT");
    }

    [Fact]
    public void ShouldReturnSameValueWhenNameIsAlreadyUppercase()
    {
        var result = TenantNormalizer.NormalizeName("TENANT");

        result.ShouldBe("TENANT");
    }

    [Fact]
    public void ShouldReturnEmptyStringWhenNameIsEmpty()
    {
        var result = TenantNormalizer.NormalizeName(string.Empty);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldPreserveSpacesWhenNormalizingName()
    {
        var result = TenantNormalizer.NormalizeName("my tenant");

        result.ShouldBe("MY TENANT");
    }

    [Fact]
    public void ShouldPreserveNumbersWhenNormalizingName()
    {
        var result = TenantNormalizer.NormalizeName("tenant123");

        result.ShouldBe("TENANT123");
    }

    [Fact]
    public void ShouldUseInvariantCultureWhenNormalizingName()
    {
        // In Turkish culture, ToUpper("i") = "İ" (dotted I), but ToUpperInvariant gives "I"
        var result = TenantNormalizer.NormalizeName("title");

        result.ShouldBe("TITLE");
    }
}