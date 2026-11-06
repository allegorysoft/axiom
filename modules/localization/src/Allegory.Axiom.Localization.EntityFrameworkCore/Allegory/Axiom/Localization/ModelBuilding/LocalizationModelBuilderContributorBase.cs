using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Localization;

public abstract class LocalizationModelBuilderContributorBase : IModelBuilderContributor
{
    public void Contribute(ModelBuilder builder, DbContext context)
    {
        if (!builder.TenancySide.HasFlag(TenancySide.Host))
        {
            return;
        }

        builder.Entity<LocalizationRecord>(e =>
        {
            e.Property(p => p.Name)
                .HasMaxLength(LocalizationRecord.MaxNameLength)
                .IsRequired();

            e.Property(p => p.Culture)
                .HasMaxLength(LocalizationRecord.MaxCultureLength)
                .IsRequired();

            e.Property(p => p.Translations)
                .HasConversion(
                    static v => LocalizationSerializer.Instance.Serialize(v),
                    static v => LocalizationSerializer.Instance.Deserialize(v));

            e.HasIndex(p => new {p.Name, p.Culture}).IsUnique();
        });

        ContributeCore(builder, context);
    }

    protected abstract void ContributeCore(ModelBuilder builder, DbContext context);
}