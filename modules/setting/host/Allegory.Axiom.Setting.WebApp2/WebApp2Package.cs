using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.Setting;
using Allegory.Axiom.UnitOfWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal sealed class WebApp2Package : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        ConfigureAxiom(builder);

        return Task.CompletedTask;
    }

    private static void ConfigureAxiom(IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureAxiomDbContext<SettingDbContext>(o =>
        {
            o.Configure(b => b.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        });
    }

    public static async Task InitializeAsync(IHost host)
    {
        await TryCreateDatabaseAsync(host);

        var app = host.GetWebApplication();

        app.UseRequestLocalization();
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseMultiTenancy();
        app.UseAuthorization();
    }

    private static async Task TryCreateDatabaseAsync(IHost host)
    {
        var manager = host.Services.GetRequiredService<IUnitOfWorkManager>();
        var provider = host.Services.GetRequiredService<IDbContextProvider<SettingDbContext>>();

        await using var uow = manager.Begin();

        var context = await provider.GetAsync();
        await context.Database.EnsureCreatedAsync();

        await uow.CompleteAsync();
    }
}