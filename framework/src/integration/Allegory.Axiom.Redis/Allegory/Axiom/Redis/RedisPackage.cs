using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Redis;

public class RedisPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<RedisOptions>(
            builder.Configuration.GetSection("Axiom:Redis"));

        return Task.CompletedTask;
    }
}