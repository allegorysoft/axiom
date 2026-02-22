using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Allegory.Axiom.Hosting.Plugins;

public class AxiomApplicationDirectoryPlugin : IAxiomApplicationPlugin
{
    public IEnumerable<Assembly> Assemblies { get; }

    public AxiomApplicationDirectoryPlugin(string directory, bool recursive = true)
    {
        var patterns = new[] {"*.dll", "*.exe"};
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        Assemblies = patterns
            .SelectMany(pattern => Directory.EnumerateFiles(directory, pattern, searchOption))
            .Distinct()
            .Select(p => AssemblyLoadContext.Default.LoadFromAssemblyPath(p))
            .ToArray();
    }

    public IEnumerable<Assembly> GetAssemblies()
    {
        return Assemblies;
    }
}