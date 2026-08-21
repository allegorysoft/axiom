using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EntityFrameworkCore;

internal sealed class EntityFrameworkCoreTestsPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureAxiomDbContexts(o =>
        {
            o.DefaultBuilderAction = b => { b.UseSqlite(); };
        });

        return Task.CompletedTask;
    }
}