using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.FileProvider;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Localization;
using Allegory.Axiom.MultiTenancy.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.MultiTenancy;

internal sealed class MultiTenancyPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<FileProviderOptions>(options =>
        {
            options.AddEmbedded<MultiTenancyPackage>();
        });

        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<LocalizationResource>(
                defaultCulture: "en",
                paths: ["Allegory/Axiom/MultiTenancy/Localization/Resources"]);

            options.MapExceptionCode<LocalizationResource>(MultiTenancyExceptionCodes.Resource);
        });

        builder.Services.Configure<CacheOptions>(options =>
        {
            options.Types[typeof(TenantContext)] = new CacheTypeOptions
            {
                Name = "tenants",
                IsTenantAgnostic = true
            };
        });

        return Task.CompletedTask;
    }
}