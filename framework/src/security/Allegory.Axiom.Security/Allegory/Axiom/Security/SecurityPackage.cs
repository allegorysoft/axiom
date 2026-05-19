using System.Threading.Tasks;
using Allegory.Axiom.FileProviders;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Localization;
using Allegory.Axiom.Security.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Security;

internal sealed class SecurityPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.AddEmbedded<SecurityPackage>();
        });

        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<LocalizationResource>(
                defaultCulture: "en",
                paths: ["Allegory/Axiom/Security/Localization/Resources"]);

            options.MapExceptionCode<LocalizationResource>(SecurityExceptionCodes.Resource);
        });

        return Task.CompletedTask;
    }
}