using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // dotnet ef migrations add Initial --context AppDbContext -o Migrations/App
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite()
            .Options;

        return new AppDbContext(options);
    }
}