using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.DistributedLocking;

internal sealed class DistributedLockingPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<DistributedLockOptions>(
            builder.Configuration.GetSection(DistributedLockOptions.Section));

        return Task.CompletedTask;
    }
}