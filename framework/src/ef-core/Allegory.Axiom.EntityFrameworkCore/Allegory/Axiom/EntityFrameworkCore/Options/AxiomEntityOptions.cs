using System;
using System.Linq;
using Allegory.Axiom.Domain.Entities;

namespace Allegory.Axiom.EntityFrameworkCore;

public class AxiomEntityOptions<TEntity> where TEntity : IEntity
{
    public static AxiomEntityOptions<TEntity> Empty { get; } = new();

    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? IncludeDetails { get; set; }
}