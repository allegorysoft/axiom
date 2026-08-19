using System;
using Allegory.Axiom.Data;
using Allegory.Axiom.Domain.Entities;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

[TenancySide(TenancySide.Hybrid)]
[ConnectionStringName("Module3")]
public class Module3DbContext : DbContext
{
    public DbSet<Module3Entity1> Entity1 { get; set; }
    public DbSet<Module3Entity2> Entity2 { get; set; }
}

public class Module3Entity1 : Entity<int> { }

public class Module3Entity2 : Entity<int>, ITenantOwned
{
    public Guid? TenantId { get; }
}