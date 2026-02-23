using System.Collections.Generic;
using System.Reflection;

namespace Allegory.Axiom.Hosting.Plugins;

public interface IAxiomApplicationPlugin
{
    IEnumerable<Assembly> GetAssemblies();
}