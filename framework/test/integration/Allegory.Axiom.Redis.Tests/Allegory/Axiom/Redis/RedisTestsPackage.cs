using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.Redis;

namespace Allegory.Axiom.Redis;

internal sealed class RedisTestsPackage : IConfigureApplication
{
    public const string SecondConnectionName = "second";

    public static async Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        var container = new RedisBuilder("redis:latest").Build();

        await builder.AddTestContainerAsync(container);

        builder.Services.Configure<RedisOptions>(o =>
        {
            var option = new RedisOption
            {
                Configuration = container.GetConnectionString()
            };

            o[RedisOptions.DefaultConnectionName] = option;

            // Multiple connections can connect to the same Redis server.
            // Each connection creates its own TCP connection.
            o[SecondConnectionName] = option;
        });
    }
}