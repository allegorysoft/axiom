using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Setting;

internal sealed class SettingEntityFrameworkCorePackage : IConfigureApplication
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