using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.EntityFrameworkCore;

public abstract class AxiomDbContextOptions(Type type)
{
    public Type Type { get; } = type;
    public Action<DbContextOptionsBuilder>? BuilderAction { get; set; }
    public string? ConnectionStringName { get; set; }
    public TenancySide TenancySide { get; internal set; }
    public IReadOnlySet<Type>? ReplacedDbContexts { get; internal set; }
}

public class AxiomDbContextOptions<TContext>()
    : AxiomDbContextOptions(typeof(TContext))
    where TContext : DbContext { }

public class AxiomDbContextOptionsBuilder
{
    private readonly HashSet<Type> _repositories = [];

    public Action<DbContextOptionsBuilder>? BuilderAction { get; set; }
    public string? ConnectionStringName { get; set; }
    public TenancySide? TenancySide { get; set; }
    public IReadOnlySet<Type>? ReplacedDbContexts { get; set; }
    public IReadOnlySet<Type> Repositories => _repositories;
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;

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
    public bool ExposeGenericRepositories { get; set; }

    public void AddRepository(Type type)
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

        _repositories.Add(type);
    }
}