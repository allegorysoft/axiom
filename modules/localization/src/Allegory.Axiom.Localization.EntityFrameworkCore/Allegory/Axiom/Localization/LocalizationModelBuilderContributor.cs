using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Localization;

public class LocalizationModelBuilderContributor : IModelBuilderContributor
{
    public static readonly IModelBuilderContributor Instance = new LocalizationModelBuilderContributor();

    private LocalizationModelBuilderContributor() { }

    public void Contribute(ModelBuilder builder, DbContext context)
    {
        if (!builder.TenancySide.HasFlag(TenancySide.Host))
        {
            return;
        }

        builder.Entity<LocalizationRecord>(e =>
        {
            e.ToTable(DbDefaults.TablePrefix + "Localizations", DbDefaults.Schema);
        });
    }
}