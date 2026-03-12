using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DependencyInjection.Proxy;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom.Castle;

public class ProxyGeneratorCastleAdapterTests
{
    [Fact]
    public async Task Test()
    {
        //CreateClassProxy, CreateClassProxy<>, CreateClassProxyWithTarget
        //CreateInterfaceProxyWithTarget, CreateInterfaceProxyWithTargetInterface
        //CreateInterfaceProxyWithoutTarget, CreateInterfaceProxyWithoutTarget<> 
        //AsyncInterceptorBase, AsyncDeterminationInterceptor : IInterceptor
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddInterceptor<Interceptor1>(
            t => typeof(IImplementation).IsAssignableFrom(t));

        // builder.Services.AddInterceptor<Interceptor2>(
        //     t => typeof(IImplementation).IsAssignableFrom(t));

        await builder.ConfigureApplicationAsync();

        var host = builder.Build();

        using var scope = host.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredKeyedService<IImplementation>(1);
        await service.TestIntAsync();

        var scope2 = host.Services.CreateScope();
        var service2 = scope2.ServiceProvider.GetRequiredKeyedService<IImplementation>(1);
        await service2.TestIntAsync();

        var service3 = scope.ServiceProvider.GetRequiredKeyedService<IImplementation>(1);
        await service3.TestIntAsync();
    }
}

public class Interceptor1 : IAxiomInterceptor, ITransientService
{
    public Guid Id { get; } = Guid.NewGuid();

    public async Task InterceptAsync(IAxiomInterceptorContext context)
    {
        await context.ProceedAsync();
    }
}

public class Somet : IScopedService
{
    public Guid Id = Guid.NewGuid();
}

public interface IImplementation
{
    void Test();
    Task TestAsync();
    Task<int> TestIntAsync();
}

[Dependency<IImplementation>(ServiceLifetime.Transient, ServiceKey = 1)]
public class Implementation : IImplementation
{
    public ISome Some { get; }
    public Implementation([FromKeyedServices(1)] ISome some)
    {
        Some = some;
    }

    public virtual void Test()
    {
        Console.WriteLine("Test executed...");
    }

    public virtual async Task TestAsync()
    {
        await Task.Delay(5000);
        Console.WriteLine("Test async executed...");
    }

    public Task<int> TestIntAsync()
    {
        Console.WriteLine("Test int async executed...");
        return Task.FromResult(1);
    }
}

public interface ISome {}

[Dependency<ISome>(ServiceLifetime.Singleton, ServiceKey = 1)]
public class Soe : ISome {}