using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public abstract class AxiomDbContextOptions(Type type)
{
    public Type Type { get; } = type;
    public Action<IServiceProvider, DbContextOptionsBuilder>? BuilderAction { get; internal set; }
    public string ConnectionStringName { get; internal set; } = null!;
    public TenancySide TenancySide { get; internal set; }

    internal Dictionary<Type, object> EntityOptions { get; } = [];

    public void Configure(Action<DbContextOptionsBuilder> action)
    {
        BuilderAction = (_, b) => action(b);
    }

    public void Configure(Action<IServiceProvider, DbContextOptionsBuilder> action)
    {
        BuilderAction = action;
    }

    public void Entity<TEntity>(Action<AxiomEntityOptions<TEntity>> action) where TEntity : IEntity
    {
        if (!EntityOptions.TryGetValue(typeof(TEntity), out var options))
        {
            EntityOptions[typeof(TEntity)] = options = new AxiomEntityOptions<TEntity>();
        }

        action((AxiomEntityOptions<TEntity>) options);
    }

    public AxiomEntityOptions<TEntity> GetEntityOptions<TEntity>() where TEntity : IEntity
    {
        if (EntityOptions.TryGetValue(typeof(TEntity), out var options))
        {
            return (AxiomEntityOptions<TEntity>) options;
        }

        return AxiomEntityOptions<TEntity>.Empty;
    }
}

public class AxiomDbContextOptions<TContext>()
    : AxiomDbContextOptions(typeof(TContext))
    where TContext : DbContext { }