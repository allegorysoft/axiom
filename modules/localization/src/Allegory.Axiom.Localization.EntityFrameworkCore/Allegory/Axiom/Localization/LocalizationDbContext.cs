using System.Collections.Generic;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Localization;

public class LocalizationDbContext(
    DbContextOptions<LocalizationDbContext> options) :
    DbContext(options),
    IModelBuilderContributorProvider
{
    public static IList<IModelBuilderContributor> Contributors { get; } =
    [
        LocalizationModelBuilderContributor.Instance
    ];

    public DbSet<LocalizationRecord> Localizations => Set<LocalizationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}