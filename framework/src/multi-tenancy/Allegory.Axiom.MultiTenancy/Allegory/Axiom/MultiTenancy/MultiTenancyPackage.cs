using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.MultiTenancy;

internal sealed class MultiTenancyPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ConfigurationTenantOptions>(
            builder.Configuration.GetSection("Axiom"));

        return Task.CompletedTask;
    }
}