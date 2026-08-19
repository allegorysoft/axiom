using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore.Repositories;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EntityFrameworkCore;

internal sealed class EntityFrameworkCorePackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.AddDeferredAction(static b =>
        {
            foreach (var registrar in RepositoryRegistrarBase.Registrars)
            {
                registrar.Value.Register();
            }

            RepositoryRegistrarBase.Registrars.Clear();
            RepositoryRegistrarBase.GenericRegistrars.Clear();
        });

        return Task.CompletedTask;
    }
}