using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Data;

internal sealed class DataPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ConnectionStringContextsOptions>(options =>
            options.Contexts = builder.Configuration
                .GetSection("Axiom:ConnectionStringContexts")
                .Get<HashSet<ConnectionStringContextOptions>>());

        return Task.CompletedTask;
    }
}