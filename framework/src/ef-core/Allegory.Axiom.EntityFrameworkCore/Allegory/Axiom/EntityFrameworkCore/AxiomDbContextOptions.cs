using System;
using System.Collections.Generic;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public abstract class AxiomDbContextOptions(Type type)
{
    public Type Type { get; } = type;
    public Action<DbContextOptionsBuilder>? BuilderAction { get; internal set; }
    public string? ConnectionStringName { get; internal set; }
    public TenancySide TenancySide { get; internal set; }
    public IReadOnlySet<Type>? ReplacedDbContexts { get; internal set; }
}

public class AxiomDbContextOptions<TContext>()
    : AxiomDbContextOptions(typeof(TContext))
    where TContext : DbContext { }