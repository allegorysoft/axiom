using System.Threading.Tasks;
using Allegory.Axiom.FileProviders;
using Allegory.Axiom.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
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
    public async Task Test()
    {
        var f = LocalizerFactory.Create(typeof(AxiomLocalizationResource));
    }
}