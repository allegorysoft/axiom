using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Interception;

public class ServiceInterceptorBinderTests
{
    [Fact]
    public void ShouldApplyInterceptors()
    {
        var collection = new ServiceCollection();

        collection.AddTransient<IImplementation1, Implementation1>();
        collection.AddScoped<IImplementation2, Implementation2>();
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t) || typeof(IImplementation2).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        collection.Single(c => c.ServiceType == typeof(IImplementation1)).ImplementationFactory.ShouldNotBeNull();
        collection.Single(c => c.ServiceType == typeof(IImplementation2)).ImplementationFactory.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldNotApplyInterceptorsWhenNoMatchingServices()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<IImplementation2, Implementation2>();
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        var service = collection.Single(c => c.ServiceType == typeof(IImplementation2));
        service.ImplementationType.ShouldBe(typeof(Implementation2));
        service.ImplementationFactory.ShouldBeNull();
    }

    [Fact]
    public void ShouldApplyMultipleInterceptorsToSameService()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<IImplementation1, Implementation1>();
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t));
        collection.AddInterceptor<Interceptor2>(t => typeof(IImplementation1).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        collection.Count(c => c.ServiceType == typeof(IImplementation1)).ShouldBe(1);
        collection.Single(c => c.ServiceType == typeof(IImplementation1)).ImplementationFactory.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldPreserveLifetimeWhenApplyingInterceptors()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<IImplementation1, Implementation1>();
        collection.AddScoped<IImplementation2, Implementation2>();
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t) || typeof(IImplementation2).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        collection.Single(c => c.ServiceType == typeof(IImplementation1)).Lifetime.ShouldBe(ServiceLifetime.Transient);
        collection.Single(c => c.ServiceType == typeof(IImplementation2)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ShouldPreserveServiceKeyWhenApplyingInterceptorsToKeyedService()
    {
        var collection = new ServiceCollection();
        const string key = "my-key";
        collection.AddKeyedScoped<IImplementation1, Implementation1>(key);
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        var service = collection.Single(c => c.ServiceType == typeof(IImplementation1));
        service.IsKeyedService.ShouldBeTrue();
        service.ServiceKey.ShouldBe(key);
        service.KeyedImplementationFactory.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldSkipInterceptorWhenServiceHasNoImplementationType()
    {
        var collection = new ServiceCollection();

        Func<IServiceProvider, Implementation1> factory = _ => new Implementation1();
        collection.AddTransient<IImplementation1>(factory);

        var instance = new Implementation2();
        collection.AddSingleton<IImplementation2>(instance);

        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t) || typeof(IImplementation2).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        var implementation1 = collection.Single(c => c.ServiceType == typeof(IImplementation1));
        implementation1.ImplementationType.ShouldBeNull();
        implementation1.ImplementationFactory.ShouldBe(factory);
        implementation1.ImplementationInstance.ShouldBeNull();

        var implementation2 = collection.Single(c => c.ServiceType == typeof(IImplementation2));
        implementation2.ImplementationType.ShouldBeNull();
        implementation2.ImplementationFactory.ShouldBeNull();
        implementation2.ImplementationInstance.ShouldBe(instance);
    }

    [Fact]
    public void ShouldSkipInterceptorWhenServiceTypeIsNotInterface()
    {
        var collection = new ServiceCollection();

        collection.AddTransient<Implementation1>();
        collection.AddSingleton<Implementation2>();
        collection.AddInterceptor<Interceptor1>(t => t == typeof(Implementation1) || t == typeof(Implementation2));

        ServiceInterceptorBinder.Apply(collection);

        var implementation1 = collection.Single(c => c.ServiceType == typeof(Implementation1));
        implementation1.ImplementationType.ShouldBe(typeof(Implementation1));
        implementation1.ImplementationFactory.ShouldBeNull();

        var implementation2 = collection.Single(c => c.ServiceType == typeof(Implementation2));
        implementation2.ImplementationType.ShouldBe(typeof(Implementation2));
        implementation2.ImplementationFactory.ShouldBeNull();
    }

    [Fact]
    public void ShouldHandleEmptyInterceptorList()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<Implementation1>();

        ServiceInterceptorBinder.Apply(collection);

        var service = collection.Single(c => c.ServiceType == typeof(Implementation1));
        service.ImplementationType.ShouldBe(typeof(Implementation1));
        service.ImplementationFactory.ShouldBeNull();
    }

    [Fact]
    public void ShouldHandleEmptyServiceCollection()
    {
        var collection = new ServiceCollection();
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t));

        Should.NotThrow(() => ServiceInterceptorBinder.Apply(collection));
        collection.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldReplaceOriginalServiceNotAddDuplicate()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<Implementation1>();
        collection.AddInterceptor<Interceptor1>(t => typeof(IImplementation1).IsAssignableFrom(t));

        ServiceInterceptorBinder.Apply(collection);

        collection.Count(c => c.ServiceType == typeof(Implementation1)).ShouldBe(1);
    }
}

file interface IImplementation1 {}

file class Implementation1 : IImplementation1 {}

file interface IImplementation2 {}

file class Implementation2 : IImplementation2 {}

file class Interceptor1 : IAxiomInterceptor
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}

file class Interceptor2 : IAxiomInterceptor
{
    public Task InterceptAsync(IAxiomInterceptorContext context) => context.ProceedAsync();
}