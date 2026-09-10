using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class AxiomDbContextsOptions
{
    private readonly HashSet<Type> _contexts = new();

    public Action<IServiceProvider, DbContextOptionsBuilder>? SharedBuilderAction { get; internal set; }
    public Action<IServiceProvider, DbContextOptionsBuilder>? BuilderAction { get; internal set; }
    public IReadOnlySet<Type> Contexts => _contexts;

    internal void AddContext(Type contextType)
    {
        _contexts.Add(contextType);
    }

    public void ConfigureShared(Action<DbContextOptionsBuilder> action)
    {
        SharedBuilderAction = (_, b) => action(b);
    }

    public void ConfigureShared(Action<IServiceProvider, DbContextOptionsBuilder> action)
    {
        SharedBuilderAction = action;
    }

    public void Configure(Action<DbContextOptionsBuilder> action)
    {
        BuilderAction = (_, b) => action(b);
    }

    public void Configure(Action<IServiceProvider, DbContextOptionsBuilder> action)
    {
        BuilderAction = action;
    }
}