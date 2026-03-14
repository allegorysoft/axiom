using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Allegory.Axiom.Hosting;

public class AxiomApplicationBuilder
{
    protected AxiomApplicationBuilderContext Context { get; set; } = null!;

    public virtual async ValueTask<AxiomApplication> BuildAsync(
        AxiomApplicationBuilderContext context)
    {
        Context = context;
        var application = await BuildAsync();
        Context.Builder.Services.AddSingleton(application);
        return application;
    }

    protected virtual async ValueTask<AxiomApplication> BuildAsync()
    {
        var assemblies = GetDependencies().ToList();
        assemblies.AddRange(GetPlugins());

        await ConfigureApplicationAsync(assemblies);
        await PostConfigureApplicationAsync(assemblies);
        Context.Builder.Services.ExecutePostConfigureActions();

        var application = new AxiomApplication(Guid.NewGuid(), Context.StartupAssembly, assemblies);
        return application;
    }

    protected virtual IEnumerable<Assembly> GetDependencies()
    {
        var dependencyContext = DependencyContext.Load(Context.StartupAssembly);
        if (dependencyContext != null)
        {
            return GetDependencies(dependencyContext);
        }

        var packages = new List<Assembly>();
        foreach (var assembly in Context.StartupAssembly.GetReferencedAssemblies())
        {
            packages.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(assembly));
        }
        packages.Add(Context.StartupAssembly);
        return packages;
    }

    protected virtual IEnumerable<Assembly> GetDependencies(DependencyContext context)
    {
        const string dependencyInjectionAssemblyName = "Allegory.Axiom.DependencyInjection.Abstractions";

        var checkedLibraries = new Dictionary<string, bool>();

        foreach (var library in context.RuntimeLibraries)
        {
            if (!HasTransitiveDependency(context, library, dependencyInjectionAssemblyName, checkedLibraries))
            {
                continue;
            }

            foreach (var assemblyName in library.GetDefaultAssemblyNames(context))
            {
                yield return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            }
        }
    }

    private bool HasTransitiveDependency(
        DependencyContext context,
        RuntimeLibrary library,
        string targetName,
        Dictionary<string, bool> checkedLibraries)
    {
        if (checkedLibraries.TryGetValue(library.Name, out var result))
        {
            return result;
        }

        // To include DependencyInjection.Abstractions, add " || library.Name == targetName" to the condition
        if (library.Dependencies.Any(d => d.Name == targetName))
        {
            return checkedLibraries[library.Name] = true;
        }

        foreach (var dependency in library.Dependencies)
        {
            var dependencyLibrary = context.RuntimeLibraries.FirstOrDefault(d => d.Name == dependency.Name);
            if (dependencyLibrary != null &&
                HasTransitiveDependency(context, dependencyLibrary, targetName, checkedLibraries))
            {
                return checkedLibraries[library.Name] = true;
            }
        }

        return checkedLibraries[library.Name] = false;
    }

    protected virtual IEnumerable<Assembly> GetPlugins()
    {
        var plugins = new List<Assembly>();
        foreach (var plugin in Context.Plugins)
        {
            plugins.AddRange(plugin.GetAssemblies());
        }
        return plugins;
    }

    protected virtual async ValueTask ConfigureApplicationAsync(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            Context.DependencyRegistrar.Register(assembly);

            var configureMethod = assembly.GetTypes().SingleOrDefault(
                    t => typeof(IConfigureApplication).IsAssignableFrom(t) &&
                         t is {IsClass: true, IsAbstract: false})?
                .GetMethod(nameof(IConfigureApplication.ConfigureAsync));

            if (configureMethod != null)
            {
                await (ValueTask) configureMethod.Invoke(null, [Context.Builder])!;
            }
        }
    }

    protected virtual async ValueTask PostConfigureApplicationAsync(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var configureMethod = assembly.GetTypes().SingleOrDefault(
                    t => typeof(IPostConfigureApplication).IsAssignableFrom(t) &&
                         t is {IsClass: true, IsAbstract: false})?
                .GetMethod(nameof(IPostConfigureApplication.PostConfigureAsync));

            if (configureMethod != null)
            {
                await (ValueTask) configureMethod.Invoke(null, [Context.Builder])!;
            }
        }
    }
}