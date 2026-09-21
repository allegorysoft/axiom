using System;
using System.Collections.Generic;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Entities.Auditing;
using Allegory.Axiom.EntityFrameworkCore.ModelBuilding;
using Allegory.Axiom.Extensibility;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

public class Module2DbContext : DbContext, IModelBuilderContributorProvider
{
    public static IList<IModelBuilderContributor> Contributors { get; } = [new Module2DbContextModelBuilderContributor()];

    public DbSet<Module2Entity1> Entity1 { get; set; }
    public DbSet<Module2Entity2> Entity2 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}

public class Module2DbContextModelBuilderContributor : IModelBuilderContributor
{
    public void Contribute(ModelBuilder modelBuilder, DbContext dbContext)
    {
        if (!modelBuilder.TenancySide.HasFlag(TenancySide.Tenant))
        {
            return;
        }

        modelBuilder.Entity<Module2Entity1>();
        modelBuilder.Entity<Module2Entity2>();
    }
}

public class Module2Entity1 : Entity<int>,
    ICreationAudited, IModificationAudited, IDeletionAudited,
    ITenantOwned,
    IConcurrencyCheck,
    IExtraProperties
{
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public DateTime? ModifiedAt { get; private set; }
    public string? ModifiedBy { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public Guid? TenantId { get; private set; }

    public uint Revision { get; set; }

    public IDictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
}

public class Module2Entity2 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}