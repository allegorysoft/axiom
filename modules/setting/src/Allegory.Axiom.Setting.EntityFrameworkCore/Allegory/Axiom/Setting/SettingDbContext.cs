using System.Collections.Generic;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Setting;

public class SettingDbContext(
    DbContextOptions<SettingDbContext> options) :
    DbContext(options),
    IModelBuilderContributorProvider
{
    public static IList<IModelBuilderContributor> Contributors { get; } =
    [
        SettingRelationalModelBuilderContributor.Instance
    ];

    public DbSet<SettingDefinitionRecord> SettingDefinitions => Set<SettingDefinitionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}