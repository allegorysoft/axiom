using System;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

[TenancySide(TenancySide.Tenant)]
[ConnectionStringName("Module2")]
public class Module2DbContext : DbContext
{
    public DbSet<Module2Entity1> Entity1 { get; set; }
    public DbSet<Module2Entity2> Entity2 { get; set; }
}

public class Module2Entity1 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}

public class Module2Entity2 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}