using System;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class Module2DbContext : DbContext
{
    public DbSet<Module2Entity1> Entity1 { get; set; }
    public DbSet<Module2Entity2> Entity2 { get; set; }
}

public class Module2Entity1 : ITenantOwned
{
    public int Id { get; set; }
    public Guid? TenantId { get; }
}

public class Module2Entity2 : ITenantOwned
{
    public int Id { get; set; }
    public Guid? TenantId { get; }
}