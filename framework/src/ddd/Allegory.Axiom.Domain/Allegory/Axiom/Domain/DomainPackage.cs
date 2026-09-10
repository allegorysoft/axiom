using System.Threading.Tasks;
using Allegory.Axiom.Domain.Localization;
using Allegory.Axiom.FileProvider;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Domain;

internal sealed class DomainPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.AddEmbedded<DomainPackage>();
        });
        
        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<LocalizationResource>(
                defaultCulture: "en",
                paths: ["Allegory/Axiom/Domain/Localization/Resources"]);

            options.MapExceptionCode<LocalizationResource>(DomainExceptionCodes.Resource);
        });

        return Task.CompletedTask;
    }
}