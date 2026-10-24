using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

internal sealed class LocalizationEntityFrameworkCoreMongoDbPackage : IConfigureApplication
{
    static LocalizationEntityFrameworkCoreMongoDbPackage()
    {
        LocalizationDbContext.Contributors.Remove(LocalizationModelBuilderContributor.Instance);
        LocalizationDbContext.Contributors.Add(MongoDbLocalizationModelBuilderContributor.Instance);
    }

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.UseMongoDbContextProvider<LocalizationDbContext>();

        return Task.CompletedTask;
    }
}