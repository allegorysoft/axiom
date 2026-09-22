using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class Module3DbContext : DbContext, IModelBuilderContributorProvider
{
    public static IList<IModelBuilderContributor> Contributors { get; } = [new Module3DbContextModelBuilderContributor()];

    public DbSet<Module3Entity1> Entity1 { get; set; }
    public DbSet<Module3Entity2> Entity2 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}

public class Module3DbContextModelBuilderContributor : IModelBuilderContributor
{
    public void Contribute(ModelBuilder modelBuilder, DbContext dbContext)
    {
        if (modelBuilder.TenancySide.HasFlag(TenancySide.Host))
        {
            modelBuilder.Entity<Module3Entity1>();
        }

        if (modelBuilder.TenancySide.HasFlag(TenancySide.Tenant))
        {
            modelBuilder.Entity<Module3Entity2>();
        }
    }
}

public class Module3Entity1 : Entity<int> { }

public class Module3Entity2 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}