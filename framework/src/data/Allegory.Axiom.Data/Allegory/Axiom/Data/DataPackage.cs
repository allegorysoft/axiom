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
        {
            var contexts = builder.Configuration
                .GetSection("Axiom:ConnectionStringContexts")
                .Get<HashSet<ConnectionStringContextOptions>>();

            if (contexts != null)
            {
                foreach (var context in contexts)
                {
                    options.Contexts.Add(context);
                }
            }
        });

        return Task.CompletedTask;
    }
}