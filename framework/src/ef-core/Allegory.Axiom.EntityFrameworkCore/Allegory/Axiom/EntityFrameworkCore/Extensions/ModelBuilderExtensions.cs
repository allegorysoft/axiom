using System;
using System.Reflection;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Allegory.Axiom.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    private static readonly MethodInfo ConfigureMethod = typeof(ModelBuilderExtensions)
        .GetMethod(nameof(ConfigureAxiom), BindingFlags.NonPublic | BindingFlags.Static)!;

    extension(ModelBuilder builder)
    {
        public void ConfigureAxiom(DbContext context, bool createIndexes = true)
        {
            var parameters = new object[] {builder, context, createIndexes};

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                var method = ConfigureMethod.MakeGenericMethod(entity.ClrType);
                method.Invoke(null, parameters);
            }
        }
    }

    private static void ConfigureAxiom<TEntity>(
        ModelBuilder builder,
        DbContext context,
        bool createIndexes)
        where TEntity : class
    {
        var entityBuilder = builder.Entity<TEntity>();

        ConfigureAudit(entityBuilder);
        ConfigureQueryFilter(entityBuilder, context, createIndexes);
    }

    private static void ConfigureAudit<TEntity>(EntityTypeBuilder<TEntity> entityBuilder) where TEntity : class
    {
        if (typeof(ICreationAudited).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<DateTime>(nameof(ICreationAudited.CreatedAt))
                .HasConversion(
                    static v => v, // interceptor already uses UTC
                    static v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }

        if (typeof(IModificationAudited).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<DateTime?>(nameof(IModificationAudited.ModifiedAt))
                .HasConversion(
                    static v => v, // interceptor already uses UTC
                    static v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        }

        if (typeof(IDeletionAudited).IsAssignableFrom(typeof(TEntity)))
        {
            entityBuilder
                .Property<DateTime?>(nameof(IDeletionAudited.DeletedAt))
                .HasConversion(
                    static v => v, // interceptor already uses UTC
                    static v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
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