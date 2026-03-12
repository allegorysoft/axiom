using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DependencyInjection.Proxy;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;
using IProxyGenerator=Allegory.Axiom.DependencyInjection.Proxy.IProxyGenerator;

namespace Allegory.Axiom.Castle;

public class ProxyGeneratorCastleAdapterTests
{
    private async Task<IServiceProvider> BuildServiceProvider(Action<IHostApplicationBuilder> configure)
    {
        var builder = Host.CreateApplicationBuilder();
        configure(builder);
        await builder.ConfigureApplicationAsync();
        return builder.Build().Services;
    }

    [Fact]
    public async Task ShouldCreateInterfaceProxyWhenServiceTypeIsInterface()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<Interceptor1>(t => typeof(IImplementation).IsAssignableFrom(t));
        });

        var instance = services.GetRequiredService<IImplementation>();

        instance.ShouldNotBeNull();
        instance.GetType().Name.ShouldContain("Proxy");
        instance.ShouldBeAssignableTo<IImplementation>();
    }

    [Fact]
    public async Task ShouldCreateClassProxyWhenServiceTypeIsClass()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddTransient<Implementation>();
            builder.Services.AddInterceptor<Interceptor1>(t => t == typeof(Implementation));
        });

        var instance = services.GetRequiredService<Implementation>();

        instance.ShouldNotBeNull();
        instance.GetType().Name.ShouldContain("Proxy");
        instance.ShouldBeAssignableTo<Implementation>();
    }

    [Fact]
    public async Task ShouldInvokeInterceptorWhenCallingMethod()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<RecordingInterceptor>(t => typeof(IImplementation).IsAssignableFrom(t));
        });

        var instance = services.GetRequiredService<IImplementation>();
        instance.Execute();

        RecordingInterceptor.Invoked.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeMultipleInterceptorsInOrder()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<OrderInterceptor1>(t => typeof(IImplementation).IsAssignableFrom(t));
            builder.Services.AddInterceptor<OrderInterceptor2>(t => typeof(IImplementation).IsAssignableFrom(t));
        });

        var instance = services.GetRequiredService<IImplementation>();
        instance.Execute();

        OrderInterceptor1.InvokedTime.ShouldBeLessThan(OrderInterceptor2.InvokedTime);
    }

    [Fact]
    public async Task ShouldCacheInterceptorTypeMapping()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<Interceptor1>(t => typeof(IImplementation).IsAssignableFrom(t));
        });

        var generator = services.GetRequiredService<IProxyGenerator>()
            as ProxyGeneratorCastleAdapter;

        generator.ShouldNotBeNull();

        // resolve twice to exercise cache
        services.GetRequiredService<IImplementation>();
        services.GetRequiredService<IImplementation>();

        generator.InterceptorMapCache.ContainsKey(typeof(Interceptor1)).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnCorrectValueFromInterceptedAsyncMethod()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<Interceptor1>(t => typeof(IImplementation).IsAssignableFrom(t));
        });

        var instance = services.GetRequiredService<IImplementation>();
        var result = await instance.GetValueAsync();

        result.ShouldBe(42);
    }

    [Fact]
    public async Task ShouldProxyRegisteredAsScoped()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<ScopedInterceptor>(t => typeof(IImplementation).IsAssignableFrom(t));
        });

        var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var instance1 = scope.ServiceProvider.GetService<IImplementation>();
            var instance2 = scope.ServiceProvider.GetService<IImplementation>();
            var interceptor = scope.ServiceProvider.GetService<ScopedInterceptor>();
            
            instance1.ShouldNotBe(instance2);
        }
        
        using (var scope = scopeFactory.CreateScope())
        {
            var x = services.GetService<IImplementation>();
        }

    }
}

public interface IImplementation : ITransientService
{
    void Execute();
    Task<int> GetValueAsync();
}

public class Implementation : IImplementation
{
    public void Execute() {}
    public Task<int> GetValueAsync() => Task.FromResult(42);
}

public class Interceptor1 : IAxiomInterceptor, ISingletonService
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}

public class RecordingInterceptor : IAxiomInterceptor, ISingletonService
{
    public static bool Invoked { get; private set; }

    public Task InterceptAsync(IAxiomInterceptorContext context)
    {
        Invoked = true;
        return context.ProceedAsync();
    }
}

public class OrderInterceptor1 : IAxiomInterceptor, ISingletonService
{
    public static long InvokedTime { get; private set; }

    public Task InterceptAsync(IAxiomInterceptorContext context)
    {
        InvokedTime = Stopwatch.GetTimestamp();
        return context.ProceedAsync();
    }
}

public class OrderInterceptor2 : IAxiomInterceptor, ISingletonService
{
    public static long InvokedTime { get; private set; }

    public Task InterceptAsync(IAxiomInterceptorContext context)
    {
        InvokedTime = Stopwatch.GetTimestamp();
        return context.ProceedAsync();
    }
}

public class ScopedInterceptor : IAxiomInterceptor, IScopedService
{
    public Guid Id { get; } = Guid.NewGuid();
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}