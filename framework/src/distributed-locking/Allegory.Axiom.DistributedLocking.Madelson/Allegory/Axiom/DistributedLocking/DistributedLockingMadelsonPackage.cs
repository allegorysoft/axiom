using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Redis;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.DistributedLocking;

internal sealed class DistributedLockingMadelsonPackage : IConfigureApplication
{
    internal const string Section = DistributedLockOptions.Section + ":Madelson";

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<DistributedLockOptions>(options =>
        {
            builder.Configuration.GetSection(Section).Bind(options.Madelson);
        });

        builder.Services.TryAddSingleton<IDistributedLockProvider>(sp =>
        {
            var factory = sp.GetRequiredService<RedisConnectionFactory>();
            var options = sp.GetRequiredService<IOptions<DistributedLockOptions>>();
            var connection = factory.Get(options.Value.Madelson.ConnectionName);

            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });

        return Task.CompletedTask;
    }
}