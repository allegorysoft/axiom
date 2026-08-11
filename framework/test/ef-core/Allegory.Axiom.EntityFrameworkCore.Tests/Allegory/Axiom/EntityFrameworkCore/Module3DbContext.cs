using System;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class Module3DbContext : DbContext
{
    public DbSet<Module3Entity1> Entity1 { get; set; }
    public DbSet<Module3Entity2> Entity2 { get; set; }
}

public class Module3Entity1
{
    public int Id { get; set; }
}

public class Module3Entity2 : ITenantOwned
{
    public int Id { get; set; }
    public Guid? TenantId { get; }
}