using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Setting;

public class SettingRelationalModelBuilderContributor : SettingModelBuilderContributorBase
{
    public static readonly IModelBuilderContributor Instance = new SettingRelationalModelBuilderContributor();

    private SettingRelationalModelBuilderContributor() { }

    protected override void ContributeCore(ModelBuilder builder, DbContext context)
    {
        if (SettingDefaults.IsSynchronizationEnabled)
        {
            builder.Entity<SettingDefinitionRecord>(e =>
            {
                e.ToTable(SettingDefaults.TablePrefix + "SettingDefinitions", SettingDefaults.Schema);
            });
        }

        builder.Entity<SettingValueRecord>(e =>
        {
            e.ToTable(SettingDefaults.TablePrefix + "SettingValues", SettingDefaults.Schema);
        });
    }
}