using System;
using System.Collections.Generic;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore;

public class AxiomDbContextOptionsBuilder
{
    internal AxiomDbContextOptionsBuilder(Type dbContextType)
    {
        DbContextType = dbContextType;
        TenancySide = TenancySideAttribute.Find(DbContextType) ?? TenancySide.Hybrid;
        ReplacedDbContexts = ReplaceDbContextAttribute.Find(DbContextType);
        SetConnectionStringName();
    }

    internal Type DbContextType { get; }
    internal HashSet<(Type Type, TenancySide? TenancySide)> Repositories { get; } = [];

    public Action<DbContextOptionsBuilder>? BuilderAction { get; set; }
    public string? ConnectionStringName { get; private set; }
    public TenancySide TenancySide { get; internal set; }
    public IReadOnlySet<Type>? ReplacedDbContexts { get; }
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// When <see langword="true"/>, allows the entity's generic <see cref="IRepository{TEntity, TKey}"/>
    /// registration to be replaced by a subsequently registered non-generic <see cref="DbContext"/>.
    /// This lets a later <see cref="DbContext"/> take ownership of the entity's repository resolution
    /// instead of the one that originally registered it. When <see langword="false"/>, the entity's generic
    /// repository remains bound to its original registering <see cref="DbContext"/> and cannot be
    /// overridden by another context.
    /// If a custom repository has already been registered for the entity via <see cref="AddRepository"/>,
    /// there is no need to set this to <see langword="true"/> it is already considered a generic
    /// <see cref="DbContext"/> registration.
    /// </summary>
    public bool RegisterAsGenericDbContext { get; set; }

    /// <summary>
    /// When <see langword="true"/>, automatically registers <see cref="IRepository{TEntity, TKey}"/>
    /// for entities that do not have a custom repository registered via <see cref="AddRepository"/>.
    /// When <see langword="false"/>, entities without a custom repository will not have any
    /// repository registered.
    /// </summary>
    public bool RegisterDefaultRepositories { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, also registers the underlying <see cref="IRepository{TEntity, TKey}"/>
    /// interface for entities that have a custom repository registered via <see cref="AddRepository"/>
    /// (e.g. resolving <c>IRepository&lt;Product, int&gt;</c> in addition to <c>IProductRepository</c>).
    /// Both interfaces resolve to the same instance. Has no effect on entities without a custom repository.
    /// </summary>
    public bool ExposeGenericServices { get; set; }

    private void SetConnectionStringName()
    {
        ConnectionStringName = ConnectionStringNameAttribute.Find(DbContextType);

        if (ConnectionStringName != null)
        {
            return;
        }

        var name = DbContextType.Name;
        ConnectionStringName = name.EndsWith("DbContext", StringComparison.OrdinalIgnoreCase)
            ? name[..^9]
            : name;
    }

    public void AddRepository(Type type, TenancySide? tenancySide = null)
    {
        if (!typeof(IRepository).IsAssignableFrom(type))
        {
            throw new ArgumentException(
                $"Repository type '{type.Name}' should implement '{nameof(IRepository)}'", nameof(type));
        }

        if (!type.IsGenericType)
        {
            throw new ArgumentException(
                $"Repository type '{type.Name}' should be a generic type that takes DbContext as a generic parameter",
                nameof(type));
        }

        Repositories.Add((type, tenancySide));
    }
}