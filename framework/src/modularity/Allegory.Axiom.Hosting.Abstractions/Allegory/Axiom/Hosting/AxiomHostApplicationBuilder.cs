using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public class AxiomHostApplicationBuilder
{
    public virtual ValueTask<AxiomHostApplication> BuildAsync(
        IHostApplicationBuilder builder,
        Assembly startupAssembly)
    {
        var dependencyContext = DependencyContext.Load(startupAssembly);
        var packages = dependencyContext == null ? GetPackages(startupAssembly) : GetPackages(dependencyContext);
        var application = new AxiomHostApplication(Guid.NewGuid(), startupAssembly, packages);

        return ValueTask.FromResult(application);
    }

    protected virtual List<Assembly> GetPackages(Assembly startupAssembly)
    {
        var packages = new List<Assembly>();

        foreach (var assembly in startupAssembly.GetReferencedAssemblies())
        {
            packages.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(assembly));
        }

        packages.Add(startupAssembly);

        return packages;
    }

    protected virtual List<Assembly> GetPackages(DependencyContext context)
    {
        const string dependencyInjectionAssemblyName = "Allegory.Axiom.DependencyInjection.Abstractions";

        var checkedLibraries = new Dictionary<string, bool>();
        var packages = new List<Assembly>();

        foreach (var library in context.RuntimeLibraries)
        {
            if (!HasTransitiveDependency(context, library, dependencyInjectionAssemblyName, checkedLibraries))
            {
                continue;
            }

            foreach (var assemblyName in library.GetDefaultAssemblyNames(context))
            {
                packages.Add(AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName));
            }
        }

        return packages;
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
}