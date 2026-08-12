using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Allegory.Axiom.EntityFrameworkCore;

public class DbContextFactory : IDesignTimeDbContextFactory<Module1DbContext>
{
    public Module1DbContext CreateDbContext(string[] args)
    {
        // dotnet ef migrations add initial --context Module1DbContext -o Migrations/Module1
        var options = new DbContextOptionsBuilder<Module1DbContext>()
            .UseSqlite("Data Source=module1.db")
            .Options;

        return new Module1DbContext(options);
    }
}