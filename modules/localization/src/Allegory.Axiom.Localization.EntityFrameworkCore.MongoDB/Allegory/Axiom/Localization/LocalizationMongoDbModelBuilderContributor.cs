using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Allegory.Axiom.Localization;

public class LocalizationMongoDbModelBuilderContributor : LocalizationModelBuilderContributorBase
{
    public static readonly IModelBuilderContributor Instance = new LocalizationMongoDbModelBuilderContributor();

    private LocalizationMongoDbModelBuilderContributor() { }

    protected override void ContributeCore(ModelBuilder builder, DbContext context)
    {
        builder.Entity<LocalizationRecord>(e =>
        {
            e.ToCollection(DbDefaults.TablePrefix + "Localizations");
        });
    }
}