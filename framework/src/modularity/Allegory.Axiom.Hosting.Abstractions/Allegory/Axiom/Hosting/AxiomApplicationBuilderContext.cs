using System.Collections.Generic;
using System.Reflection;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting.Plugins;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public record AxiomApplicationBuilderContext(
    IHostApplicationBuilder Builder,
    Assembly StartupAssembly,
    AssemblyDependencyRegistrar DependencyRegistrar,
    List<IAxiomApplicationPlugin> Plugins);