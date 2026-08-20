using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class AxiomDbContextGlobalOptions
{
    public Action<DbContextOptionsBuilder>? SharedBuilderAction { get; set; }
    public Action<DbContextOptionsBuilder>? DefaultBuilderAction { get; set; }
    public HashSet<Type> Contexts { get; } = [];
}