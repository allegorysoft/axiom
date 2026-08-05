using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Data;

internal sealed class DataPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ConnectionStringOptions>(
            builder.Configuration.GetSection("Axiom:ConnectionStrings"));

        return Task.CompletedTask;
    }
}