using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

[TenancySide(TenancySide.Host)]
[ConnectionStringName("Module1")]
public class Module1DbContext(DbContextOptions<Module1DbContext> options) : DbContext(options)
{
    public DbSet<Module1Entity1> Entity1 { get; set; }
    public DbSet<Module1Entity2> Entity2 { get; set; }
}

public class Module1Entity1 : AggregateRoot<int> { }

public class Module1Entity2 : AggregateRoot<int> { }

public interface IModule1Entity1Repository : IRepository<Module1Entity1, int> { }

public interface IModule1Entity2Repository : IRepository<Module1Entity2, int> { }

public interface IModule1ReportRepository : IRepository { }

public class EfCoreModule1Entity1Repository<TDbContext>(
    IDbContextProvider<TDbContext> dbContextProvider)
    : EfCoreRepository<TDbContext, Module1Entity1, int>(dbContextProvider), IModule1Entity1Repository
    where TDbContext : DbContext { }

public class EfCoreModule1Entity2Repository<TDbContext>(
    IDbContextProvider<TDbContext> dbContextProvider)
    : EfCoreRepository<TDbContext, Module1Entity2, int>(dbContextProvider), IModule1Entity2Repository
    where TDbContext : DbContext { }

public class EfCoreModule1ReportRepository<TContext>(
    IDbContextProvider<TContext> dbContextProvider) : IModule1ReportRepository
    where TContext : DbContext { }