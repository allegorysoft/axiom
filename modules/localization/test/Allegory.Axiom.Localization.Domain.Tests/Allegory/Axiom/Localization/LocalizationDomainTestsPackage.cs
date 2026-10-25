using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom.Localization;

internal sealed class LocalizationDomainTestsPackage : IConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureDbContext<LocalizationDbContext>(b =>
        {
            b.UseSqlite($"Data Source={TestContext.Current.TestClass!.TestClassName + ".db"}");
        });

        return Task.CompletedTask;
    }

    public static async Task InitializeAsync(IHost host)
    {
        var manager = host.Services.GetRequiredService<IUnitOfWorkManager>();
        await using var uow = manager.Begin();

        var provider = host.Services.GetRequiredService<IDbContextProvider<LocalizationDbContext>>();
        var dbContext = await provider.GetAsync();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        await uow.CompleteAsync();
    }
}