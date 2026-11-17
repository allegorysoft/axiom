using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Localization;

public class LocalizationRelationalModelBuilderContributor : LocalizationModelBuilderContributorBase
{
    public static readonly IModelBuilderContributor Instance = new LocalizationRelationalModelBuilderContributor();

    private LocalizationRelationalModelBuilderContributor() { }

    protected override void ContributeCore(ModelBuilder builder, DbContext context)
    {
        builder.Entity<LocalizationRecord>(e =>
        {
            e.ToTable(DbDefaults.TablePrefix + "Localizations", DbDefaults.Schema);
        });
    }
}