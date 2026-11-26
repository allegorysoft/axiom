using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Setting;
using Allegory.Axiom.UnitOfWork;
using DefaultNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
await app.InitializeApplicationAsync();
await app.RunAsync();

internal sealed class AppPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureAxiomDbContext<SettingDbContext>(o =>
        {
            o.Configure(b => b.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        });

        return Task.CompletedTask;
    }

    public static async Task InitializeAsync(IHost host)
    {
        await MigrateDatabase(host);
        await Test(host);
    }

    static async Task MigrateDatabase(IHost host)
    {
        var manager = host.Services.GetRequiredService<IUnitOfWorkManager>();
        var provider = host.Services.GetRequiredService<IDbContextProvider<SettingDbContext>>();

        await using var uow = manager.Begin();

        var context = await provider.GetAsync();
        await context.Database.EnsureCreatedAsync();

        await uow.CompleteAsync();
    }

    static async Task Test(IHost host)
    {
        var manager = host.Services.GetRequiredService<ISettingValueManager>();
        const string setting = "Setting1";

        var v = await manager.FindAsync(setting);
        var v2 = await manager.FindAsync<ComplexSetting>(setting);
        var v3 = await manager.FindAsync(setting);

        await manager.SetAsync(
            setting,
            new SettingValueIdentifier(ISettingValueContributor.Tenant, null),
            new ComplexSetting() {Host = "127.0.0.1", Port = 12345}, false);
    }
}