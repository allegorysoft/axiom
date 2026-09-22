using System;
using System.Collections.Generic;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class Module1DbContext : DbContext, IModelBuilderContributorProvider
{
    public static IList<IModelBuilderContributor> Contributors { get; } = [new Module1DbContextModelBuilderContributor()];

    public DbSet<Module1Entity1> Entity1 { get; set; }
    public DbSet<Module1Entity2> Entity2 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureAxiom(this);
    }
}

public class Module1DbContextModelBuilderContributor : IModelBuilderContributor
{
    public void Contribute(ModelBuilder modelBuilder, DbContext dbContext)
    {
        if (!modelBuilder.TenancySide.HasFlag(TenancySide.Host))
        {
            return;
        }

        modelBuilder.Entity<Module1Entity1>();
        modelBuilder.Entity<Module1Entity2>();
    }
}

public class Module1Entity1 : AggregateRoot<int> { }

public class Module1Entity2 : AggregateRoot<int> { }

public interface IModule1Entity1Repository : IRepository<Module1Entity1, int> { }

public interface IModule1Entity2Repository : IRepository<Module1Entity2, int> { }

public interface IModule1ReportRepository : IRepository { }

public class EfCoreModule1Entity1Repository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, Module1Entity1, int>(serviceProvider),
    IModule1Entity1Repository
    where TDbContext : DbContext { }

public class EfCoreModule1Entity2Repository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, Module1Entity2, int>(serviceProvider),
    IModule1Entity2Repository
    where TDbContext : DbContext { }

public class EfCoreModule1ReportRepository<TContext>(
    IDbContextProvider<TContext> dbContextProvider) :
    IModule1ReportRepository
    where TContext : DbContext
{
    protected IDbContextProvider<TContext> DbContextProvider { get; } = dbContextProvider;
}