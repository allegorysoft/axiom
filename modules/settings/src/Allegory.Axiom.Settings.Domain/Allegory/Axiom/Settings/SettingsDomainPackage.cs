using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Settings.Synchronization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Settings;

internal sealed class SettingsDomainPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<SettingSynchronizerWorker>();

        return Task.CompletedTask;
    }
}