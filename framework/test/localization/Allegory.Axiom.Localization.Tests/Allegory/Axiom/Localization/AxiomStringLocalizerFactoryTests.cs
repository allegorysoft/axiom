using System.Threading.Tasks;
using Allegory.Axiom.FileProviders;
using Allegory.Axiom.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Localization;

public class AxiomStringLocalizerFactoryTests : HostedIntegrationTestBase
{
    protected IStringLocalizerFactory LocalizerFactory => Service<IStringLocalizerFactory>();

    protected override ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.AddEmbedded<AxiomStringLocalizerFactoryTests>();
        });

        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<AxiomLocalizationResource>(
                "en",
                "/Allegory/Axiom/Localization/Resources/Directory-1",
                "/Allegory/Axiom/Localization/Resources/Directory-2");
        });

        return ValueTask.CompletedTask;
    }

    [Fact]
    public void ShouldCreateStringLocalizerForRegisteredResourceType()
    {
        var localizer = LocalizerFactory.Create(typeof(AxiomLocalizationResource));

        localizer.ShouldNotBeNull();
        localizer.ShouldBeOfType<AxiomStringLocalizer>();
    }

    [Fact]
    public void ShouldCreateStringLocalizerByResourceName()
    {
        var resourceName = typeof(AxiomLocalizationResource).FullName!;

        var localizer = LocalizerFactory.Create(resourceName, string.Empty);

        localizer.ShouldNotBeNull();
        localizer.ShouldBeOfType<AxiomStringLocalizer>();
    }

    [Fact]
    public void ShouldReturnSameStringLocalizerOnSubsequentCallsForSameResourceType()
    {
        var first = LocalizerFactory.Create(typeof(AxiomLocalizationResource));
        var second = LocalizerFactory.Create(typeof(AxiomLocalizationResource));

        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void ShouldReturnSameStringLocalizerOnSubsequentCallsForSameResourceName()
    {
        var resourceName = typeof(AxiomLocalizationResource).FullName!;

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
        var factory = (AxiomStringLocalizerFactory)LocalizerFactory;

        LocalizerFactory.Create(typeof(AxiomLocalizationResource));

        factory.LocalizerCache.ContainsKey(typeof(AxiomLocalizationResource).FullName!).ShouldBeTrue();
    }

    [Fact]
    public void ShouldCacheLocalizerCreatedByResourceName()
    {
        var factory = (AxiomStringLocalizerFactory)LocalizerFactory;
        var resourceName = typeof(AxiomLocalizationResource).FullName!;

        LocalizerFactory.Create(resourceName, string.Empty);

        factory.LocalizerCache.ContainsKey(resourceName).ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnSameLocalizerInstanceWhenCreatedByResourceTypeAndResourceName()
    {
        var baseName = typeof(AxiomLocalizationResource).FullName!;

        var byType = LocalizerFactory.Create(typeof(AxiomLocalizationResource));
        var byName = LocalizerFactory.Create(baseName, string.Empty);

        byType.ShouldBeSameAs(byName);
    }
}

file class UnregisteredLocalizationResource {}