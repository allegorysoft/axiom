using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public static class HostExtensions
{
    extension(IHost host)
    {
        public async Task InitializeApplicationAsync()
        {
            //TODO: Add concurrent parameter

            var application = host.Services.GetRequiredService<AxiomApplication>();

            foreach (var assembly in application.Assemblies)
            {
                var configureMethod = assembly.GetTypes()
                    .SingleOrDefault(t => typeof(IInitializeApplication).IsAssignableFrom(t)
                                          && t is {IsClass: true, IsAbstract: false})?
                    .GetMethod(nameof(IInitializeApplication.InitializeAsync));

                if (configureMethod != null)
                {
                    await (Task) configureMethod.Invoke(null, [host])!;
                }
            }
        }
    }
}