using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Settings;

internal sealed class SettingsDomainPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<SettingDefinitionSynchronizerWorker>();

        return Task.CompletedTask;
    }
}