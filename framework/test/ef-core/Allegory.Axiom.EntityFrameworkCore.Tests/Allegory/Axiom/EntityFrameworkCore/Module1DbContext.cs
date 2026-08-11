using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public class Module1DbContext(DbContextOptions<Module1DbContext> options) : DbContext(options)
{
    public DbSet<Module1Entity1> Entity1 { get; set; }
    public DbSet<Module1Entity2> Entity2 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ConfigureModule
        // ConfigureAxiom at last
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
}

public class Module1Entity1
{
    public int Id { get; set; }
}

public class Module1Entity2
{
    public int Id { get; set; }
}