using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

internal sealed class LocalizationDomainPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(LocalizationSerializer.Instance);
        builder.Services.AddHostedService<LocalizationContextWorker>();

        return Task.CompletedTask;
    }
}