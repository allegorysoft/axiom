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
    private readonly HashSet<Type> _repositories = [];

    public Action<DbContextOptionsBuilder>? BuilderAction { get; set; }
    public string? ConnectionStringName { get; set; }
    public TenancySide? TenancySide { get; set; }
    public IReadOnlyList<Type>? ReplacedDbContexts { get; set; }

    public IReadOnlySet<Type> Repositories => _repositories;

    public void AddRepository(Type type)
    {
        if (!typeof(IRepository).IsAssignableFrom(type))
        {
            throw new ArgumentException("Repository type should implement 'IRepository'", nameof(type));
        }

        _repositories.Add(type);
    }
}