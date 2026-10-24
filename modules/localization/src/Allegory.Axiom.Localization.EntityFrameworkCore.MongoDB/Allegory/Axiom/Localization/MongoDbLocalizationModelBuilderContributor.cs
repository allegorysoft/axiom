using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Allegory.Axiom.Localization;

public class MongoDbLocalizationModelBuilderContributor : IModelBuilderContributor
{
    public static readonly IModelBuilderContributor Instance = new MongoDbLocalizationModelBuilderContributor();

    private MongoDbLocalizationModelBuilderContributor() { }

    public void Contribute(ModelBuilder builder, DbContext context)
    {
        if (!builder.TenancySide.HasFlag(TenancySide.Host))
        {
            return;
        }

        builder.Entity<LocalizationRecord>(e =>
        {
            e.ToCollection(DbDefaults.TablePrefix + "Localizations");
        });
    }
}