using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Allegory.Axiom.Hosting.Plugins;

public class AxiomApplicationDirectoryPlugin : IAxiomApplicationPlugin
{
    protected static readonly string[] Patterns = ["*.dll", "*.exe"];

    public AxiomApplicationDirectoryPlugin(string directory, bool recursive = true)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        Assemblies = Patterns
            .SelectMany(pattern => Directory.EnumerateFiles(directory, pattern, searchOption))
            .Distinct()
            .Select(p => AssemblyLoadContext.Default.LoadFromAssemblyPath(p))
            .ToArray();
    }

    public IEnumerable<Assembly> Assemblies { get; }

    public IEnumerable<Assembly> GetAssemblies()
    {
        return Assemblies;
    }
}