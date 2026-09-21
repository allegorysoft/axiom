using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.EntityFrameworkCore.ModelBuilding;
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

public class Module2Entity1 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}

public class Module2Entity2 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}