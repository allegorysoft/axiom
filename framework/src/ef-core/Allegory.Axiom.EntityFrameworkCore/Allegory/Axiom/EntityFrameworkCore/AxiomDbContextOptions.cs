using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public abstract class AxiomDbContextOptions(Type type)
{
    public Type Type { get; } = type;
    public Action<DbContextOptionsBuilder>? BuilderAction { get; set; }
    public string? ConnectionStringName { get; set; }
    public TenancySide TenancySide { get; internal set; }
    public IReadOnlyList<Type>? ReplacedDbContexts { get; internal set; }
}

public class AxiomDbContextOptions<TContext>()
    : AxiomDbContextOptions(typeof(TContext))
    where TContext : DbContext { }

public class AxiomDbContextOptionsBuilder
{
    private readonly Dictionary<Type, Type> _repositories = new();

    public Action<DbContextOptionsBuilder>? BuilderAction { get; set; }
    public string? ConnectionStringName { get; set; }
    public TenancySide? TenancySide { get; set; }
    public IReadOnlyList<Type>? ReplacedDbContexts { get; set; }

    public IReadOnlyDictionary<Type, Type> Repositories => _repositories;

    public void AddRepository<TEntity, TRepository>()
        where TEntity : IEntity
        where TRepository : IRepository
    {
        _repositories.Add(typeof(TEntity), typeof(TRepository));
    }
}