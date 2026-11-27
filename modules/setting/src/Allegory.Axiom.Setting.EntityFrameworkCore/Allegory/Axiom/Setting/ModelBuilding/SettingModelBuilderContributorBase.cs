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

        if (SettingDefaults.IsSynchronizationEnabled)
        {
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
                if (builder.ShouldCreateIndexes)
                {
                    e.HasIndex(p => new {p.Name}).IsUnique();
                }
            });
        }
        else
        {
            builder.Ignore<SettingDefinitionRecord>();
        }

        builder.Entity<SettingValueRecord>(e =>
        {
            e.Property(p => p.Id).HasColumnOrder(0);

            e.Property(p => p.Name)
                .HasColumnOrder(1)
                .HasMaxLength(SettingValueRecord.MaxNameLength)
                .IsRequired();

            e.ComplexProperty(p => p.Identifier, identifier =>
            {
                identifier.Property(p => p.Name)
                    .HasColumnOrder(2)
                    .HasColumnName("IdentifierName")
                    .HasMaxLength(SettingValueRecord.MaxIdentifierNameLength)
                    .IsRequired();

                identifier.Property(p => p.Key)
                    .HasColumnOrder(3)
                    .HasColumnName("IdentifierKey")
                    .HasMaxLength(SettingValueRecord.MaxIdentifierKeyLength);
            });

            e.Property(p => p.Value)
                .HasColumnOrder(4)
                .HasMaxLength(SettingValueRecord.MaxValueLength)
                .IsRequired();

            if (builder.ShouldCreateIndexes)
            {
                e.HasIndex(p => new
                {
                    SettingName = p.Name,
                    IdentifierName = p.Identifier.Name,
                    IdentifierKey = p.Identifier.Key
                }).IsUnique();

                e.HasIndex(p => new
                {
                    p.Identifier.Name,
                    p.Identifier.Key
                });
            }
        });

        ContributeCore(builder, context);
    }

    protected abstract void ContributeCore(ModelBuilder builder, DbContext context);
}