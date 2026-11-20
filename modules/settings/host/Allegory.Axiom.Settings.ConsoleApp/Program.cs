using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Settings;
using Allegory.Axiom.Settings.Synchronization;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZiggyCreatures.Caching.Fusion;

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

        builder.AddBuilderAction<IFusionCacheBuilder>(b =>
        {
            b.WithOptions(options => { options.DistributedCacheKeyModifierMode = CacheKeyModifierMode.None; });
        });

        return Task.CompletedTask;
    }

    public static async Task InitializeAsync(IHost host)
    {
        await MigrateDatabase(host);
        await SynchronizeAsync(host);
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

    static async Task SynchronizeAsync(IHost host)
    {
        var synchronizer = host.Services.GetRequiredService<SettingDefinitionSynchronizer>();
        await synchronizer.SynchronizeAsync();

        var store = host.Services.GetRequiredService<ISettingDefinitionStore>();

        var settings = await store.GetListAsync();
        var settings2 = await store.GetListAsync();
        var settings3 = await store.GetListAsync();
    }
}