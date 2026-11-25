using System.Threading.Tasks;
using Allegory.Axiom.FileProvider;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Localization;
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

        builder.Services.Configure<FileProviderOptions>(options => { options.AddEmbedded<SettingPackage>(); });

        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<LocalizationResource>(
                defaultCulture: "en",
                paths: ["Allegory/Axiom/Setting/Localization/Resources"]);

            options.MapExceptionCode<LocalizationResource>(SettingExceptionCodes.Resource);
        });

        return Task.CompletedTask;
    }
}