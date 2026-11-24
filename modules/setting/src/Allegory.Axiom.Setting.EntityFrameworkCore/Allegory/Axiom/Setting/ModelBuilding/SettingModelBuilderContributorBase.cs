using System.Collections.Immutable;
using System.Text.Json;
using Allegory.Axiom.EntityFrameworkCore;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Allegory.Axiom.Setting;

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
                .HasMaxLength(SettingDefinitionRecord.MaxDefaultValue);

            e.Property(p => p.Contributors)
                .HasConversion(
                    static v => v.Count > 0 ? JsonSerializer.Serialize(v) : string.Empty,
                    static v => string.IsNullOrWhiteSpace(v)
                        ? ImmutableSortedSet<string>.Empty
                        : JsonSerializer.Deserialize<ImmutableSortedSet<string>>(v) ??
                          ImmutableSortedSet<string>.Empty,
                    ValueComparer.CreateDefault<ImmutableSortedSet<string>>(favorStructuralComparisons: false));

            e.HasIndex(p => new {p.Name}).IsUnique();
        });

        builder.Entity<SettingValueRecord>(e =>
        {
            e.Property(p => p.Name)
                .HasMaxLength(SettingValueRecord.MaxNameLength)
                .IsRequired();

            e.Property(p => p.ContributorName)
                .HasMaxLength(SettingValueRecord.MaxContributorNameLength)
                .IsRequired();

            e.Property(p => p.ContributorKey)
                .HasMaxLength(SettingValueRecord.MaxContributorKeyLength);

            e.Property(p => p.Value)
                .HasMaxLength(SettingValueRecord.MaxValueLength)
                .IsRequired();

            e.HasIndex(p => new {p.Name, p.ContributorName, p.ContributorKey}).IsUnique();
        });

        ContributeCore(builder, context);
    }

    protected abstract void ContributeCore(ModelBuilder builder, DbContext context);
}