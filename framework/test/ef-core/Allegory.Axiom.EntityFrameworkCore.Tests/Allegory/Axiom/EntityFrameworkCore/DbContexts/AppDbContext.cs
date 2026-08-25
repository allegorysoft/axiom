using System;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppEntity1> Entity1 => Set<AppEntity1>();
}

public class AppEntity1 : AggregateRoot<int>, ICreationAudited, IModificationAudited, IDeletionAudited, ITenantOwned
{
    protected AppEntity1() { }

    public AppEntity1(string number)
    {
        Number = number;
    }

    public string Number { get; private init; } = null!;
    public DateTime CreatedAt { get; private init; }
    public string? CreatedBy { get; private init; }
    public DateTime? ModifiedAt { get; private init; }
    public string? ModifiedBy { get; private init; }
    public bool IsDeleted { get; private init; }
    public DateTime? DeletedAt { get; private init; }
    public string? DeletedBy { get; private init; }
    public Guid? TenantId { get; private init; }
}