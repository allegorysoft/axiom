using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.Extensibility;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Allegory.Axiom.EntityFrameworkCore.ModelBuilding;

public class GlobalModelBuilder
{
    public static GlobalModelBuilder Instance { get; set; } = new();

    protected static readonly MethodInfo GetContributorProviderMethod = typeof(IModelBuilderContributorProvider)
        .GetProperty(nameof(IModelBuilderContributorProvider.Contributors))!
        .GetMethod!;

    protected GlobalModelBuilder() { }

    public IList<IModelBuilderContributor> Contributors { get; } = [];

    public virtual void Build(ModelBuilder builder, DbContext context)
    {
        ApplyContributors(builder, context);
        ConfigureConventions(builder, context);
        ApplyGlobalContributors(builder, context);
    }

    protected virtual void ApplyContributors(ModelBuilder builder, DbContext context)
    {
        var contextType = context.GetType();

        ApplyContributorsForContext(contextType, builder, context);

        var contexts = ReplaceDbContextAttribute.Find(contextType);
        if (contexts == null)
        {
            return;
        }

        foreach (var type in contexts)
        {
            ApplyContributorsForContext(type, builder, context);
        }
    }

    protected virtual void ConfigureConventions(ModelBuilder builder, DbContext context)
    {
        var parameters = new object[] {builder, context};
        var configureMethod = GetType()
            .GetMethod(
                nameof(ConfigureEntity),
                BindingFlags.Instance | BindingFlags.NonPublic)!;

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var method = configureMethod.MakeGenericMethod(entity.ClrType);
            method.Invoke(this, parameters);
        }
    }

    protected virtual void ApplyGlobalContributors(ModelBuilder builder, DbContext context)
    {
        foreach (var contributor in Contributors)
        {
            contributor.Contribute(builder, context);
        }
    }

    protected virtual void ApplyContributorsForContext(Type contextType, ModelBuilder builder, DbContext context)
    {
        if (!typeof(IModelBuilderContributorProvider).IsAssignableFrom(contextType))
        {
            return;
        }

        var interfaceMap = contextType.GetInterfaceMap(typeof(IModelBuilderContributorProvider));
        var getContributorMethodInfo = interfaceMap
            .InterfaceMethods
            .Select((method, index) => (method, index))
            .Where(x => x.method == GetContributorProviderMethod)
            .Select(x => interfaceMap.TargetMethods[x.index])
            .Single();

        var contributors = (IList<IModelBuilderContributor>) getContributorMethodInfo.Invoke(null, null)!;

        foreach (var contributor in contributors)
        {
            contributor.Contribute(builder, context);
        }
    }

    protected virtual void ConfigureEntity<TEntity>(
        ModelBuilder builder,
        DbContext context)
        where TEntity : class
    {
        var entityBuilder = builder.Entity<TEntity>();

        ConfigureProperties(entityBuilder);
        ConfigureQueryFilter(entityBuilder, context);
    }

    protected virtual void ConfigureProperties<TEntity>(
        EntityTypeBuilder<TEntity> entityBuilder)
        where TEntity : class
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

    protected virtual void ConfigureQueryFilter<TEntity>(
        EntityTypeBuilder<TEntity> entityBuilder,
        DbContext context)
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

        if (!entityBuilder.Metadata.Model.ShouldCreateIndexes)
        {
            return;
        }

        switch (isTenantOwned)
        {
            case true when isSoftDelete:
                entityBuilder.HasIndex(nameof(ITenantOwned.TenantId), nameof(ISoftDelete.IsDeleted));
                break;
            case true:
                entityBuilder.HasIndex(nameof(ITenantOwned.TenantId));
                break;
        }
    }
}