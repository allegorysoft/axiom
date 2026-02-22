using System.Collections.Generic;
using System.Reflection;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting.Plugins;

namespace Allegory.Axiom.Hosting;

public class AxiomHostApplicationOptions
{
    public Assembly? StartupAssembly { get; set; }
    public AssemblyDependencyRegistrar? DependencyRegistrar { get; set; }
    public AxiomHostApplicationBuilder? ApplicationBuilder { get; set; }
    public List<IAxiomApplicationPlugin> Plugins { get; set; } = [];

    //Remote plugin

    internal AxiomHostApplicationOptions() {}
}