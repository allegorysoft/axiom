using System;
using System.Reflection;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

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