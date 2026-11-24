using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Setting;

internal sealed class SettingPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<SettingValueOptions>(o =>
        {
            o.AddContributor<ConfigurationSettingValueContributor>();
            o.AddContributor<GlobalSettingValueContributor>();
            o.AddContributor<TenantSettingValueContributor>();
            o.AddContributor<UserSettingValueContributor>();
        });

        return Task.CompletedTask;
    }
}