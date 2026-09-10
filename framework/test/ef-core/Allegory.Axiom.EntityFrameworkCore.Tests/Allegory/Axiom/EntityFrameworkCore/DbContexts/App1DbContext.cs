using System;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.Domain.Repositories;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

public class App1DbContext : DbContext
{
    public DbSet<App1Entity1> Entity1 { get; set; }
    public DbSet<App1Entity2> Entity2 { get; set; }
}

public class App1Entity1 : AggregateRoot<int> { }

public class App1Entity2 : AggregateRoot<int> { }

public interface IApp1Entity1Repository : IRepository<App1Entity1, int> { }

public class EfCoreApp1Entity1Repository(IServiceProvider serviceProvider) :
    EfCoreRepository<App1DbContext, App1Entity1, int>(serviceProvider),
    IApp1Entity1Repository { }