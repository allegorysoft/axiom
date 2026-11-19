using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text.Json;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Allegory.Axiom.Settings;

public abstract class SettingModelBuilderContributorBase : IModelBuilderContributor
{
    public void Contribute(ModelBuilder builder, DbContext context)
    {
        if (!builder.TenancySide.HasFlag(TenancySide.Host))
        {
            return;
        }

        builder.Entity<SettingDefinitionRecord>(e =>
        {
            e.Property(p => p.Name)
                .HasMaxLength(SettingDefinitionRecord.MaxNameLength)
                .IsRequired();

            e.Property(p => p.DefaultValue)
                .HasMaxLength(SettingDefinitionRecord.MaxDefaultValue)
                .IsRequired();

            e.Property(p => p.Contributors)
                .HasConversion(
                    static v => JsonSerializer.Serialize(v),
                    static v => JsonSerializer.Deserialize<ImmutableHashSet<string>>(v) ??
                                ImmutableHashSet<string>.Empty,
                    ValueComparer.CreateDefault<ImmutableHashSet<string>>(favorStructuralComparisons: false));

            e.HasIndex(p => new {p.Name}).IsUnique();
        });

        ContributeCore(builder, context);
    }

    protected abstract void ContributeCore(ModelBuilder builder, DbContext context);
}