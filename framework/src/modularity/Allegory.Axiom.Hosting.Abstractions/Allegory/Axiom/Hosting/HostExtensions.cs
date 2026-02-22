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
        public async ValueTask ConfigureApplicationAsync(
            Action<AxiomHostApplicationOptions>? optionsAction = null)
        {
            var options = new AxiomHostApplicationOptions();
            optionsAction?.Invoke(options);

            options.StartupAssembly ??= Assembly.GetEntryAssembly();
            ArgumentNullException.ThrowIfNull(options.StartupAssembly);
            options.ApplicationBuilder ??= new AxiomHostApplicationBuilder();
            options.DependencyRegistrar ??= new AssemblyDependencyRegistrar(builder.Services);

            var application = await options.ApplicationBuilder.BuildAsync(builder, options.StartupAssembly);
            builder.Services.AddSingleton(application);

            foreach (var assembly in application.Assemblies)
            {
                options.DependencyRegistrar.Register(assembly);

                var configureMethod = assembly.GetTypes().SingleOrDefault(
                        t => typeof(IConfigureApplication).IsAssignableFrom(t) &&
                             t is {IsClass: true, IsAbstract: false})?
                    .GetMethod(nameof(IConfigureApplication.ConfigureAsync));

                if (configureMethod != null)
                {
                    await (ValueTask) configureMethod.Invoke(null, [builder])!;
                }
            }

            foreach (var assembly in application.Assemblies)
            {
                var configureMethod = assembly.GetTypes().SingleOrDefault(
                        t => typeof(IPostConfigureApplication).IsAssignableFrom(t) &&
                             t is {IsClass: true, IsAbstract: false})?
                    .GetMethod(nameof(IPostConfigureApplication.PostConfigureAsync));

                if (configureMethod != null)
                {
                    await (ValueTask) configureMethod.Invoke(null, [builder])!;
                }
            }
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