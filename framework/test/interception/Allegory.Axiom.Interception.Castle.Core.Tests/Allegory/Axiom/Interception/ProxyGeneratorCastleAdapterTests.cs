using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Interception;

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
        generator.InterceptorMapCache[typeof(Interceptor1)].ShouldBe(typeof(AxiomInterceptorDeterminationCastleAdapter<Interceptor1>));
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
    public async Task ShouldResolveCorrectLifetimeForInterceptors()
    {
        var services = await BuildServiceProvider(builder =>
        {
            builder.Services.AddInterceptor<TransientInterceptor>(_ => false);
            builder.Services.AddInterceptor<ScopedInterceptor>(_ => false);
            builder.Services.AddInterceptor<SingletonInterceptor>(_ => false);
        });

        var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
        AxiomInterceptorDeterminationCastleAdapter<ScopedInterceptor> scope1ScopedInterceptor, scope2ScopedInterceptor;
        AxiomInterceptorDeterminationCastleAdapter<SingletonInterceptor> scope1SingletonInterceptor, scope2SingletonInterceptor;

        using (var scope = scopeFactory.CreateScope())
        {
            scope1ScopedInterceptor = scope.ServiceProvider.GetRequiredService<AxiomInterceptorDeterminationCastleAdapter<ScopedInterceptor>>();
            scope1SingletonInterceptor = scope.ServiceProvider.GetRequiredService<AxiomInterceptorDeterminationCastleAdapter<SingletonInterceptor>>();

            var transient1 = scope.ServiceProvider.GetRequiredService<AxiomInterceptorDeterminationCastleAdapter<TransientInterceptor>>();
            var transient2 = scope.ServiceProvider.GetRequiredService<AxiomInterceptorDeterminationCastleAdapter<TransientInterceptor>>();
            transient1.ShouldNotBe(transient2);
        }

        using (var scope = scopeFactory.CreateScope())
        {
            scope2ScopedInterceptor = scope.ServiceProvider.GetRequiredService<AxiomInterceptorDeterminationCastleAdapter<ScopedInterceptor>>();
            scope2SingletonInterceptor = scope.ServiceProvider.GetRequiredService<AxiomInterceptorDeterminationCastleAdapter<SingletonInterceptor>>();
        }

        scope1ScopedInterceptor.ShouldNotBe(scope2ScopedInterceptor);
        scope1SingletonInterceptor.ShouldBe(scope2SingletonInterceptor);
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

public class TransientInterceptor : IAxiomInterceptor, ITransientService
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}

public class ScopedInterceptor : IAxiomInterceptor, IScopedService
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}

public class SingletonInterceptor : IAxiomInterceptor, ISingletonService
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}