using Allegory.Axiom.Data.ConnectionStrings;
using Allegory.Axiom.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

[ConnectionStringName("App3AttributedConnection")]
[TenancySide(TenancySide.Tenant)]
public class App3DbContext : DbContext { }