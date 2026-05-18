using System.Threading.Tasks;
using Allegory.Axiom.FileProviders;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Localization.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

internal sealed class LocalizationTestsPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.AddEmbedded<AxiomStringLocalizerTests>();
        });

        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<LocalizationResource>(
                defaultCulture: "en",
                paths:
                [
                    "/Allegory/Axiom/Localization/Resources/Directory-1",
                    "/Allegory/Axiom/Localization/Resources/Directory-2"
                ]);
        });

        return Task.CompletedTask;
    }
}