using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public static class HostExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public async ValueTask ConfigureApplicationAsync(Action<AxiomHostApplicationOptions>? optionsAction = null)
        {
            var options = new AxiomHostApplicationOptions();
            optionsAction?.Invoke(options);

            options.StartupAssembly ??= Assembly.GetEntryAssembly();
            ArgumentNullException.ThrowIfNull(options.StartupAssembly);

            options.ApplicationBuilder ??= new AxiomHostApplicationBuilder();
            await options.ApplicationBuilder.BuildAsync(
                new AxiomHostApplicationBuilderContext(
                    builder,
                    options.StartupAssembly,
                    options.DependencyRegistrar ??= new AssemblyDependencyRegistrar(builder.Services),
                    options.Plugins));
        }
    }

    extension(IHost host)
    {
        public async ValueTask InitializeApplicationAsync()
        {
            //TODO: Add concurrent parameter

            var application = host.Services.GetRequiredService<AxiomHostApplication>();

            foreach (var assembly in application.Assemblies)
            {
                var configureMethod = assembly.GetTypes().SingleOrDefault(
                        t => typeof(IInitializeApplication).IsAssignableFrom(t) &&
                             t is {IsClass: true, IsAbstract: false})?
                    .GetMethod(nameof(IInitializeApplication.InitializeAsync));

                if (configureMethod != null)
                {
                    await (ValueTask) configureMethod.Invoke(null, [host])!;
                }
            }
        }
    }
}