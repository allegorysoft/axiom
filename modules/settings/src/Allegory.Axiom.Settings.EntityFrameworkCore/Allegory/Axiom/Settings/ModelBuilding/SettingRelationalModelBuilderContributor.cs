using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Settings;

public class SettingRelationalModelBuilderContributor : SettingModelBuilderContributorBase
{
    public static readonly IModelBuilderContributor Instance = new SettingRelationalModelBuilderContributor();

    private SettingRelationalModelBuilderContributor() { }

    protected override void ContributeCore(ModelBuilder builder, DbContext context)
    {
        builder.Entity<SettingDefinitionRecord>(e =>
        {
            e.ToTable(DbDefaults.TablePrefix + "Settings", DbDefaults.Schema);
        });
    }
}