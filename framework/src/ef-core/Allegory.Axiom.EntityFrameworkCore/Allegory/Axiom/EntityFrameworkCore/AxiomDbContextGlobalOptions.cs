using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class AxiomDbContextGlobalOptions
{
    private readonly HashSet<Type> _contexts = new();

    public Action<DbContextOptionsBuilder>? SharedBuilderAction { get; set; }
    public Action<DbContextOptionsBuilder>? DefaultBuilderAction { get; set; }
    public IReadOnlySet<Type> Contexts => _contexts;

    internal void AddContext(Type contextType)
    {
        _contexts.Add(contextType);
    }
}