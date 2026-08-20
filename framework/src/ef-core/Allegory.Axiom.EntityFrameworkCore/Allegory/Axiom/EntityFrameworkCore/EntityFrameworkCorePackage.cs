using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EntityFrameworkCore;

internal sealed class EntityFrameworkCorePackage : IConfigureApplication
{

    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.AddDeferredAction(static b =>
        {
            var properties = ServiceCollectionExtensions.CollectionProperties.GetOrCreateValue(b.Services);

            foreach (var registrar in properties.Registrars)
            {
                registrar.Value.Register();
            }
        });

        return Task.CompletedTask;
    }
}