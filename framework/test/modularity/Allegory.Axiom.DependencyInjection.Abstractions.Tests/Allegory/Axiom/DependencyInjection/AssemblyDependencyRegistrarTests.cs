using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.DependencyInjection;

public class AssemblyDependencyRegistrarTests
{
    public AssemblyDependencyRegistrarTests()
    {
        Registrar.Register(typeof(AssemblyDependencyRegistrarTests).Assembly);
    }

    protected AssemblyDependencyRegistrar Registrar { get; } = new(new ServiceCollection());

    [Fact]
    public void RegisterClassesWithCorrectLifetimeViaMarkerInterfaces()
    {
        var transient = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(TransientProductManager));
        transient.ShouldNotBeNull();
        transient.ImplementationType.ShouldBe(typeof(TransientProductManager));
        transient.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var scoped = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(ScopedProductManager));
        scoped.ShouldNotBeNull();
        scoped.ImplementationType.ShouldBe(typeof(ScopedProductManager));
        scoped.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var singleton = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(SingletonProductManager));
        singleton.ShouldNotBeNull();
        singleton.ImplementationType.ShouldBe(typeof(SingletonProductManager));
        singleton.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterDerivedClassWithInheritedLifetimeViaMarkerInterface()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(InheritedProductManager));

        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(InheritedProductManager));
        service.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterGenericClassViaMarkerInterface()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(GenericProductManager<>));

        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(GenericProductManager<>));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfacesWithCorrectLifetimeViaMarkerInterfaces()
    {
        var transient = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(ITransientOrderManager));
        transient.ShouldNotBeNull();
        transient.ImplementationType.ShouldBe(typeof(TransientOrderManager));
        transient.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var scoped = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IScopedOrderManager));
        scoped.ShouldNotBeNull();
        scoped.ImplementationType.ShouldBe(typeof(ScopedOrderManager));
        scoped.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var singleton = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(ISingletonOrderManager));
        singleton.ShouldNotBeNull();
        singleton.ImplementationType.ShouldBe(typeof(ExtendedNameSingletonOrderManager));
        singleton.Lifetime.ShouldBe(ServiceLifetime.Singleton);

        // Concrete classes are also registered alongside their interfaces (3 total)
        Registrar.ServiceCollection.Count(s =>
                s.ServiceType == typeof(TransientOrderManager) ||
                s.ServiceType == typeof(ScopedOrderManager) ||
                s.ServiceType == typeof(ExtendedNameSingletonOrderManager))
            .ShouldBe(3);
    }

    [Fact]
    public void SkipInterfaceRegistrationWhenNameDoesNotMatchViaMarkerInterface()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IOrderManager));
        service.ShouldBeNull();

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(OrderNameNotMatchedManager));
        implementation.ShouldNotBeNull();
        implementation.ImplementationType.ShouldBe(typeof(OrderNameNotMatchedManager));
        implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterGenericInterfaceViaMarkerInterface()
    {
        var openGeneric = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IOrderRepository<>));
        openGeneric.ShouldNotBeNull();
        openGeneric.ImplementationType.ShouldBe(typeof(OrderRepository<>));
        openGeneric.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var closedGeneric = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IOrderRepository<int>));
        closedGeneric.ShouldNotBeNull();
        closedGeneric.ImplementationType.ShouldBe(typeof(OrderRepository));
        closedGeneric.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterClassesWithCorrectLifetimeViaDependencyAttribute()
    {
        var transient = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedTransientProductManager));
        transient.ShouldNotBeNull();
        transient.ImplementationType.ShouldBe(typeof(AttributedTransientProductManager));
        transient.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var scoped = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedScopedProductManager));
        scoped.ShouldNotBeNull();
        scoped.ImplementationType.ShouldBe(typeof(AttributedScopedProductManager));
        scoped.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var singleton = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedSingletonProductManager));
        singleton.ShouldNotBeNull();
        singleton.ImplementationType.ShouldBe(typeof(AttributedSingletonProductManager));
        singleton.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterDerivedClassWithInheritedLifetimeViaDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(InheritedAttributedSingletonProductManager));

        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(InheritedAttributedSingletonProductManager));
        service.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterGenericClassViaDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedGenericProductManager<>));

        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(AttributedGenericProductManager<>));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfacesWithCorrectLifetimeViaDependencyAttribute()
    {
        var transient = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedTransientOrderManager));
        transient.ShouldNotBeNull();
        transient.ImplementationType.ShouldBe(typeof(AttributedTransientOrderManager));
        transient.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var scoped = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedScopedOrderManager));
        scoped.ShouldNotBeNull();
        scoped.ImplementationType.ShouldBe(typeof(AttributedScopedOrderManager));
        scoped.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var singleton = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedSingletonOrderManager));
        singleton.ShouldNotBeNull();
        singleton.ImplementationType.ShouldBe(typeof(ExtendedNameAttributedSingletonOrderManager));
        singleton.Lifetime.ShouldBe(ServiceLifetime.Singleton);

        // Concrete classes are also registered alongside their interfaces (3 total)
        Registrar.ServiceCollection.Count(s =>
                s.ServiceType == typeof(AttributedTransientOrderManager) ||
                s.ServiceType == typeof(AttributedScopedOrderManager) ||
                s.ServiceType == typeof(ExtendedNameAttributedSingletonOrderManager))
            .ShouldBe(3);
    }

    [Fact]
    public void SkipInterfaceRegistrationWhenNameDoesNotMatchViaDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedOrderManager));
        service.ShouldBeNull();

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedOrderNameNotMatchedManager));
        implementation.ShouldNotBeNull();
        implementation.ImplementationType.ShouldBe(typeof(AttributedOrderNameNotMatchedManager));
        implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterGenericInterfaceViaDependencyAttribute()
    {
        var openGeneric = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedOrderRepository<>));
        openGeneric.ShouldNotBeNull();
        openGeneric.ImplementationType.ShouldBe(typeof(AttributedOrderRepository<>));
        openGeneric.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var closedGeneric = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedOrderRepository<int>));
        closedGeneric.ShouldNotBeNull();
        closedGeneric.ImplementationType.ShouldBe(typeof(AttributedOrderRepository));
        closedGeneric.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterClassWithOverriddenLifetimeViaDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(OverriddenLifetimeProductManager));

        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(OverriddenLifetimeProductManager));
        service.ImplementationType!.GetInterface(typeof(ISingletonService).FullName!).ShouldNotBeNull();
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterKeyedServiceViaDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IKeyedProductManager));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBeNull();
        service.IsKeyedService.ShouldBeTrue();
        service.ServiceKey.ShouldBe(1);
        service.KeyedImplementationType.ShouldBe(typeof(KeyedProductManager));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(KeyedProductManager));
        implementation.ShouldNotBeNull();
        implementation.ImplementationType.ShouldBeNull();
        implementation.IsKeyedService.ShouldBeTrue();
        implementation.ServiceKey.ShouldBe(1);
        implementation.KeyedImplementationType.ShouldBe(typeof(KeyedProductManager));
        implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void SkipRegistrationWhenAutoRegisterIsFalseViaDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(SkipRegisterForThisClass));

        service.ShouldBeNull();
    }

    [Fact]
    public void RegisterInterfaceOnceWhenTryAddViaDependencyAttribute()
    {
        Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(IAttributedTransientOrderManager)).ShouldBe(1);

        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedTransientOrderManager));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(AttributedTransientOrderManager));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfaceReplacingExistingViaDependencyAttribute()
    {
        Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(ICustomerManager)).ShouldBe(1);

        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(ICustomerManager));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(ReplacedCustomerManager));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfaceViaGenericDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IFooManager));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(GenericAttributedManager));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(GenericAttributedManager));
        implementation.ShouldNotBeNull();
        implementation.ImplementationType.ShouldBe(typeof(GenericAttributedManager));
        implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfacesWithDifferentLifetimesViaGenericDependencyAttribute()
    {
        var zooService = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IZooManager));
        zooService.ShouldNotBeNull();
        zooService.ImplementationType.ShouldBe(typeof(GenericAttributedManager2));
        zooService.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var hooService = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IHooManager));
        hooService.ShouldNotBeNull();
        hooService.ImplementationType.ShouldBe(typeof(GenericAttributedManager2));
        hooService.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(GenericAttributedManager2));
        implementation.ShouldNotBeNull();
        implementation.ImplementationType.ShouldBe(typeof(GenericAttributedManager2));
        implementation.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }

    [Fact]
    public void RegisterInterfaceWhenDefaultLifetimeIsNullViaGenericDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGooManager));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(GenericAttributedManager3));
        service.Lifetime.ShouldBe(ServiceLifetime.Scoped);

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(GenericAttributedManager3));
        implementation.ShouldBeNull();
    }

    [Fact]
    public void RegisterKeyedServiceViaGenericDependencyAttribute()
    {
        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGenericAttributedKeyedService));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBeNull();
        service.IsKeyedService.ShouldBeTrue();
        service.ServiceKey.ShouldBe(1);
        service.KeyedImplementationType.ShouldBe(typeof(GenericAttributedManager4));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);

        var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(GenericAttributedManager4));
        implementation.ShouldNotBeNull();
        implementation.IsKeyedService.ShouldBeFalse();
        implementation.ImplementationType.ShouldBe(typeof(GenericAttributedManager4));
        implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfaceOnceWhenTryAddViaGenericDependencyAttribute()
    {
        Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(IGenericAttributeTryAddService)).ShouldBe(1);

        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGenericAttributeTryAddService));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(GenericAttributedManager5));
        service.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void RegisterInterfaceReplacingExistingViaGenericDependencyAttribute()
    {
        Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(IGenericAttributeReplaceService)).ShouldBe(1);

        var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGenericAttributeReplaceService));
        service.ShouldNotBeNull();
        service.ImplementationType.ShouldBe(typeof(GenericAttributedManager8));
        service.Lifetime.ShouldBe(ServiceLifetime.Singleton);
    }
}