using System;
using System.Collections.Generic;
using System.Reflection;

namespace Allegory.Axiom.Hosting;

public sealed class AxiomHostApplication(
    Guid id,
    Assembly startupAssembly,
    IReadOnlyCollection<Assembly> assemblies)
{
    public Guid Id { get; } = id;
    public Assembly StartupAssembly { get; } = startupAssembly;
    public IReadOnlyCollection<Assembly> Assemblies { get; } = assemblies;
}