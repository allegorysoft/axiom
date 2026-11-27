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
        builder.Services.Configure<SettingOptions>(o =>
        {
            o.SettingValue.AddContributor<ConfigurationSettingValueContributor>();
            o.SettingValue.AddContributor<GlobalSettingValueContributor>();
            o.SettingValue.AddContributor<TenantSettingValueContributor>();
            o.SettingValue.AddContributor<UserSettingValueContributor>();
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