using System;
using System.Collections.Generic;
using System.Reflection;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.EntityFrameworkCore.ModelBuilding;
using Allegory.Axiom.Extensibility;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    private const string TenancySideAnnotation = $"{nameof(TenancySideAnnotation)}";

    private static readonly MethodInfo ConfigureEntityMethod = typeof(ModelBuilderExtensions)
        .GetMethod(nameof(ConfigureEntity), BindingFlags.NonPublic | BindingFlags.Static)!;

    extension(ModelBuilder builder)
    {
        public void ConfigureAxiom(DbContext context, bool createIndexes = true)
        {
            var contextType = context.GetType();

            builder.SetTenancySide(contextType);
            builder.ApplyContributors(contextType, context, createIndexes);

            var parameters = new object[] {builder, context, createIndexes};

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                var method = ConfigureEntityMethod.MakeGenericMethod(entity.ClrType);
                method.Invoke(null, parameters);
            }
        }

        public TenancySide GetTenancySide()
        {
            var annotation = builder.Model.FindAnnotation(TenancySideAnnotation);
            ArgumentNullException.ThrowIfNull(annotation);
            ArgumentNullException.ThrowIfNull(annotation.Value);

            return (TenancySide) annotation.Value;
        }

        private void SetTenancySide(Type contextType)
        {
            builder.HasAnnotation(
                TenancySideAnnotation,
                TenancySideAttribute.Find(contextType) ?? TenancySide.Hybrid);
        }

        private void ApplyContributors(Type contextType, DbContext context, bool createIndexes)
        {
            ApplyContributors(contextType, builder, context, createIndexes);

            var contexts = ReplaceDbContextAttribute.Find(contextType);
            if (contexts == null)
            {
                return;
            }

            foreach (var type in contexts)
            {
                ApplyContributors(type, builder, context, createIndexes);
            }
        }
    }

    private static void ApplyContributors(
        Type contextType,
        ModelBuilder builder,
        DbContext context,
        bool createIndexes)
    {
        if (!typeof(IModelBuilderContributorProvider).IsAssignableFrom(contextType))
        {
            return;
        }

        if (contextType
                .GetProperty(
                    nameof(IModelBuilderContributorProvider.Contributors),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!
                .GetValue(null) is not IList<IModelBuilderContributor> contributors)
        {
            return;
        }

        foreach (var contributor in contributors)
        {
            contributor.Contribute(builder, context, createIndexes);
        }
    }

    private static void ConfigureEntity<TEntity>(
        ModelBuilder builder,
        DbContext context,
        bool createIndexes)
        where TEntity : class
    {
        var entityBuilder = builder.Entity<TEntity>();

        ConfigureProperties(entityBuilder);
        ConfigureQueryFilter(entityBuilder, context, createIndexes);
    }

    private static void ConfigureProperties<TEntity>(EntityTypeBuilder<TEntity> entityBuilder) where TEntity : class
    {
        if (typeof(ICreationAudited).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<DateTime>(nameof(ICreationAudited.CreatedAt))
                .HasConversion(
                    static v => v, // interceptor already uses UTC
                    static v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entityBuilder
                .Property(nameof(ICreationAudited.CreatedBy))
                .HasMaxLength(AuditingConstants.UserIdMaxLength);
        }

        if (typeof(IModificationAudited).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<DateTime?>(nameof(IModificationAudited.ModifiedAt))
                .HasConversion(
                    static v => v, // interceptor already uses UTC
                    static v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            entityBuilder
                .Property(nameof(IModificationAudited.ModifiedBy))
                .HasMaxLength(AuditingConstants.UserIdMaxLength);
        }

        if (typeof(IDeletionAudited).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<DateTime?>(nameof(IDeletionAudited.DeletedAt))
                .HasConversion(
                    static v => v, // interceptor already uses UTC
                    static v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

            entityBuilder
                .Property(nameof(IDeletionAudited.DeletedBy))
                .HasMaxLength(AuditingConstants.UserIdMaxLength);
        }

        if (typeof(IConcurrencyCheck).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property(nameof(IConcurrencyCheck.Revision))
                .IsConcurrencyToken();
        }

        if (typeof(IExtraProperties).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<IDictionary<string, object>>(nameof(IExtraProperties.ExtraProperties))
                .HasConversion(
                    static v => ExtraPropertiesManager.Instance.Serialize(v),
                    static v => ExtraPropertiesManager.Instance.Deserialize(v));
        }
    }

    private static void ConfigureQueryFilter<TEntity>(
        EntityTypeBuilder<TEntity> entityBuilder,
        DbContext context,
        bool createIndexes)
        where TEntity : class
    {
        var isSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));
        var isTenantOwned = typeof(ITenantOwned).IsAssignableFrom(typeof(TEntity));

        if (isSoftDelete)
        {
            entityBuilder.HasQueryFilter(
                nameof(ISoftDelete),
                e => EF.Property<bool>(e, nameof(ISoftDelete.IsDeleted)) == false);
        }

        if (isTenantOwned)
        {
            entityBuilder.HasQueryFilter(
                nameof(ITenantOwned),
                e => EF.Property<Guid?>(e, nameof(ITenantOwned.TenantId)) == context.GetCurrentTenantId());
        }

        if (createIndexes)
        {
            if (isTenantOwned && isSoftDelete)
            {
                entityBuilder.HasIndex(nameof(ITenantOwned.TenantId), nameof(ISoftDelete.IsDeleted));
            }
            else if (isTenantOwned)
            {
                entityBuilder.HasIndex(nameof(ITenantOwned.TenantId));
            }
        }
    }
}