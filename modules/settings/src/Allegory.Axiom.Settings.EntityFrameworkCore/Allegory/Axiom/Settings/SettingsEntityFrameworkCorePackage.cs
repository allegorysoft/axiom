using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Settings;

internal sealed class SettingsEntityFrameworkCorePackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddAxiomDbContext<SettingDbContext>(o =>
        {
            o.AddRepository(typeof(EfCoreSettingDefinitionRecordRepository<>));
        });

        return Task.CompletedTask;
    }
}