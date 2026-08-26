using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Allegory.Axiom.EntityFrameworkCore.DbContexts;

public class DbContextFactory : IDesignTimeDbContextFactory<App2DbContext>
{
    public App2DbContext CreateDbContext(string[] args)
    {
        // dotnet ef migrations add Initial --context App2DbContext -o Migrations/App
        var options = new DbContextOptionsBuilder<App2DbContext>()
            .UseSqlite()
            .Options;

        return new App2DbContext(options);
    }
}