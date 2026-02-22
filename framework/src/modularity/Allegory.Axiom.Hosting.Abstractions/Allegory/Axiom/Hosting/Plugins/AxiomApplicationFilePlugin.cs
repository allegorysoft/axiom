using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace Allegory.Axiom.Hosting.Plugins;

public class AxiomApplicationFilePlugin(string path) : IAxiomApplicationPlugin
{
    public Assembly Assembly { get; } = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    public IEnumerable<Assembly> GetAssemblies()
    {
        yield return Assembly;
    }
}