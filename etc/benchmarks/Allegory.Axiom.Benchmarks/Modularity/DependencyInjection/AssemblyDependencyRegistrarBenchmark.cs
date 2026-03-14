using System.Reflection;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Module1;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.Benchmarks.Modularity.DependencyInjection;

public class AssemblyDependencyRegistrarBenchmark
{
    private ServiceCollection _collection = null!;
    private readonly Assembly _startupAssembly = typeof(Implementation).Assembly;

    [GlobalSetup]
    public void Setup()
    {
        _collection = [];
    }

    [Benchmark(Baseline = true)]
    public void DirectRegister()
    {
        _collection.AddTransient<IImplementation, Implementation>();
        _collection.AddTransient<Implementation>();
    }

    [Benchmark]
    public void ReflectionRegister()
    {
        var serviceRegistrar = new AssemblyDependencyRegistrar(_collection);
        serviceRegistrar.Register(_startupAssembly);
    }
}