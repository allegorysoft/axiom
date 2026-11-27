using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Setting;

public sealed class SettingDomainPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        if (SettingDefaults.IsSynchronizationEnabled)
        {
            builder.Services.AddHostedService<SettingDefinitionSynchronizerWorker>();
            builder.Services.AddSingleton<ISettingDefinitionStore, SettingDefinitionStore>();
        }

        return Task.CompletedTask;
    }
}