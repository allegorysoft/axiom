using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

namespace Allegory.Axiom.Localization;

internal sealed class LocalizationPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        builder.Services.AddSingleton<ResourceManagerStringLocalizerFactory>();

        return Task.CompletedTask;
    }
}