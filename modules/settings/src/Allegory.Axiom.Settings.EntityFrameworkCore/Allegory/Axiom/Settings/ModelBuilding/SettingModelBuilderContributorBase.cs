using System.Collections.Immutable;
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
                    static v => v.Count > 0 ? JsonSerializer.Serialize(v) : string.Empty,
                    static v => string.IsNullOrWhiteSpace(v)
                        ? ImmutableSortedSet<string>.Empty
                        : JsonSerializer.Deserialize<ImmutableSortedSet<string>>(v) ??
                          ImmutableSortedSet<string>.Empty,
                    ValueComparer.CreateDefault<ImmutableSortedSet<string>>(favorStructuralComparisons: false));

            e.HasIndex(p => new {p.Name}).IsUnique();
        });

        ContributeCore(builder, context);
    }

    protected abstract void ContributeCore(ModelBuilder builder, DbContext context);
}