using System.Threading.Tasks;
using Allegory.Axiom.AspNetCore;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.FileProvider;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Localization;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Localization;
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
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.AddSupportedCultures("en", "fr");
            options.AddSupportedUICultures("en", "fr");
        });

        builder.Services.Configure<FileProviderOptions>(options => { options.AddEmbedded<WebApp2Package>(); });

        builder.Services.Configure<LocalizationOptions>(options =>
        {
            options.Resources.Add<LocalizationResource>(
                defaultCulture: "en",
                paths: ["Localization/Resources"]);
        });

        builder.Services.ConfigureAxiomDbContext<LocalizationDbContext>(o =>
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
        var provider = host.Services.GetRequiredService<IDbContextProvider<LocalizationDbContext>>();

        await using var uow = manager.Begin();

        var context = await provider.GetAsync();
        await context.Database.EnsureCreatedAsync();

        await uow.CompleteAsync();
    }
}