using Allegory.Axiom.Localization.Resources;
using Microsoft.Extensions.Localization;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizerFactoryTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IStringLocalizerFactory LocalizerFactory { get; } = fixture.Service<IStringLocalizerFactory>();

    [Fact]
    public void ShouldCreateStringLocalizerForRegisteredResourceType()
    {
        var localizer = LocalizerFactory.Create(typeof(LocalizationResource));

        localizer.ShouldNotBeNull();
        localizer.ShouldBeOfType<AxiomStringLocalizer>();
    }

    [Fact]
    public void ShouldCreateStringLocalizerByResourceName()
    {
        var resourceName = typeof(LocalizationResource).FullName!;

        var localizer = LocalizerFactory.Create(resourceName, string.Empty);

        localizer.ShouldNotBeNull();
        localizer.ShouldBeOfType<AxiomStringLocalizer>();
    }

    [Fact]
    public void ShouldReturnSameStringLocalizerOnSubsequentCallsForSameResourceType()
    {
        var first = LocalizerFactory.Create(typeof(LocalizationResource));
        var second = LocalizerFactory.Create(typeof(LocalizationResource));

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void ShouldReturnSameStringLocalizerOnSubsequentCallsForSameResourceName()
    {
        var resourceName = typeof(LocalizationResource).FullName!;

        var first = LocalizerFactory.Create(resourceName, string.Empty);
        var second = LocalizerFactory.Create(resourceName, string.Empty);

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void ShouldFallbackToResourceManagerLocalizerForUnregisteredType()
    {
        var localizer = LocalizerFactory.Create(typeof(UnregisteredLocalizationResource));

        localizer.ShouldNotBeNull();
        localizer.ShouldBeOfType<ResourceManagerStringLocalizer>();
    }

    [Fact]
    public void ShouldFallbackToResourceManagerLocalizerForUnregisteredResourceName()
    {
        var localizer = LocalizerFactory.Create("SomeUnknownBaseName", "Allegory.Axiom.Localization.Tests");

        localizer.ShouldNotBeNull();
        localizer.ShouldBeOfType<ResourceManagerStringLocalizer>();
    }

    [Fact]
    public void ShouldCacheLocalizerCreatedByResourceType()
    {
        var factory = (AxiomStringLocalizerFactory) LocalizerFactory;

        LocalizerFactory.Create(typeof(LocalizationResource));

        factory.LocalizerCache.ContainsKey(typeof(LocalizationResource).FullName!).ShouldBeTrue();
    }

    [Fact]
    public void ShouldCacheLocalizerCreatedByResourceName()
    {
        var factory = (AxiomStringLocalizerFactory) LocalizerFactory;
        var resourceName = typeof(LocalizationResource).FullName!;

        LocalizerFactory.Create(resourceName, string.Empty);

        factory.LocalizerCache.ContainsKey(resourceName).ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnSameLocalizerInstanceWhenCreatedByResourceTypeAndResourceName()
    {
        var baseName = typeof(LocalizationResource).FullName!;

        var byType = LocalizerFactory.Create(typeof(LocalizationResource));
        var byName = LocalizerFactory.Create(baseName, string.Empty);

        byType.ShouldBeSameAs(byName);
    }
}

file class UnregisteredLocalizationResource {}