using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EntityFrameworkCore;

internal sealed class EntityFrameworkCoreTestsPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureAxiomDbContextGlobalOptions(o =>
        {
            o.DefaultBuilderAction = b => { b.UseSqlite(); };
        });

        builder.Services.AddAxiomDbContext<Module1DbContext>(o =>
        {
            o.AddRepository(typeof(EfCoreModule1Entity1Repository<>));
            o.AddRepository(typeof(EfCoreModule1Entity2Repository<>));
            o.AddRepository(typeof(EfCoreModule1ReportRepository<>), TenancySide.Host);
        });

        builder.Services.AddAxiomDbContext<Module2DbContext>(o => { o.RegisterAsGenericDbContext = true; });

        builder.Services.AddAxiomDbContext<Module3DbContext>(o => { o.RegisterAsGenericDbContext = true; });

        return Task.CompletedTask;
    }
}