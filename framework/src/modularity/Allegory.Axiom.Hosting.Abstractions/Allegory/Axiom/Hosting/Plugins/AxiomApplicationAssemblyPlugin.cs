using System.Collections.Generic;
using System.Reflection;

namespace Allegory.Axiom.Hosting.Plugins;

public class AxiomApplicationAssemblyPlugin(params IEnumerable<Assembly> assemblies) : IAxiomApplicationPlugin
{
    public IEnumerable<Assembly> Assemblies { get; } = assemblies;

    public IEnumerable<Assembly> GetAssemblies()
    {
        return Assemblies;
    }
}