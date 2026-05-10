using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.DependencyInjection;

public class AssemblyDependencyRegistrarTests
{
    protected AssemblyDependencyRegistrarTests()
    {
        Registrar.Register(typeof(AssemblyDependencyRegistrarTests).Assembly);
    }

    protected AssemblyDependencyRegistrar Registrar { get; } = new(new ServiceCollection());

    public class MarkerInterfaceTests : AssemblyDependencyRegistrarTests
    {
        [Fact]
        public void ShouldRegisterClass()
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
        public void ShouldRegisterClassWhenLifetimeInheritedFromBaseClass()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(InheritedProductManager));

            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(InheritedProductManager));
            service.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }

        [Fact]
        public void ShouldRegisterGenericClass()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(GenericProductManager<>));

            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(GenericProductManager<>));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldRegisterInterfaceByNameConvention()
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
        }

        [Fact]
        public void ShouldNotRegisterInterfaceWhenNameDoesNotMatch()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IOrderManager));
            service.ShouldBeNull();

            var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(OrderNameNotMatchedManager));
            implementation.ShouldNotBeNull();
            implementation.ImplementationType.ShouldBe(typeof(OrderNameNotMatchedManager));
            implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldRegisterGenericInterface()
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
    }

    public class DependencyAttributeTests : AssemblyDependencyRegistrarTests
    {
        [Fact]
        public void ShouldRegisterClass()
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
        public void ShouldRegisterClassWhenLifetimeInheritedFromBaseClass()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(InheritedAttributedSingletonProductManager));

            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(InheritedAttributedSingletonProductManager));
            service.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }

        [Fact]
        public void ShouldRegisterGenericClass()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedGenericProductManager<>));

            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(AttributedGenericProductManager<>));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldRegisterInterfaceByNameConvention()
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
        }

        [Fact]
        public void ShouldNotRegisterInterfaceWhenNameDoesNotMatch()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedOrderManager));
            service.ShouldBeNull();

            var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(AttributedOrderNameNotMatchedManager));
            implementation.ShouldNotBeNull();
            implementation.ImplementationType.ShouldBe(typeof(AttributedOrderNameNotMatchedManager));
            implementation.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldRegisterGenericInterface()
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
        public void ShouldOverrideLifetime()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(OverriddenLifetimeProductManager));

            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(OverriddenLifetimeProductManager));
            service.ImplementationType!.GetInterface(typeof(ISingletonService).FullName!).ShouldNotBeNull();
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldRegisterAsKeyedService()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IKeyedProductManager));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBeNull();
            service.IsKeyedService.ShouldBeTrue();
            service.ServiceKey.ShouldBe(1);
            service.KeyedImplementationType.ShouldBe(typeof(KeyedProductManager));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldNotRegisterWhenAutoRegisterIsFalse()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(SkipRegisterForThisClass));

            service.ShouldBeNull();
        }

        [Fact]
        public void ShouldNotRegisterImplementationTypeWhenSelfRegisterIsFalse()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IDontSelfRegisterService));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(DontSelfRegisterService));

            var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(DontSelfRegisterService));
            implementation.ShouldBeNull();
        }

        [Fact]
        public void ShouldRegisterImplementationTypeWhenSelfRegisterIsTrue()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(ISelfRegisterService));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(SelfRegisterService));

            var implementation = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(SelfRegisterService));
            implementation.ShouldNotBeNull();
            implementation.ImplementationType.ShouldBe(typeof(SelfRegisterService));
        }

        [Fact]
        public void ShouldSkipDuplicateRegistrationWithTryAddStrategy()
        {
            Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(IAttributedTransientOrderManager)).ShouldBe(1);
            Registrar.ServiceCollection.Count(s => s.ImplementationType == typeof(TryAddAttributedTransientOrderManager)).ShouldBe(0);

            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IAttributedTransientOrderManager));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(AttributedTransientOrderManager));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldReplaceExistingRegistrationWithReplaceStrategy()
        {
            Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(ICustomerManager)).ShouldBe(1);
            Registrar.ServiceCollection.Count(s => s.ImplementationType == typeof(CustomerManager)).ShouldBe(0);

            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(ICustomerManager));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(ReplacedCustomerManager));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }
    }

    public class GenericDependencyAttributeTests : AssemblyDependencyRegistrarTests
    {
        [Fact]
        public void ShouldRegisterSpecifiedService()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IFooManager));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(GenericAttributedManager));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldRegisterServicesWithIndependentLifetimes()
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
        public void ShouldRegisterServiceWhenServiceLifetimeExplicitWithNoDefaultLifetimeOnClass()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGooManager));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(GenericAttributedManager3));
            service.Lifetime.ShouldBe(ServiceLifetime.Scoped);
        }

        [Fact]
        public void ShouldRegisterAsKeyedService()
        {
            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGenericAttributedKeyedService));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBeNull();
            service.IsKeyedService.ShouldBeTrue();
            service.ServiceKey.ShouldBe(1);
            service.KeyedImplementationType.ShouldBe(typeof(GenericAttributedManager4));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldSkipDuplicateRegistrationWithTryAddStrategy()
        {
            Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(IGenericAttributeTryAddService)).ShouldBe(1);
            Registrar.ServiceCollection.Count(s => s.ImplementationType == typeof(GenericAttributedManager6)).ShouldBe(0);

            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGenericAttributeTryAddService));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(GenericAttributedManager5));
            service.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }

        [Fact]
        public void ShouldReplaceExistingRegistrationWithReplaceStrategy()
        {
            Registrar.ServiceCollection.Count(s => s.ServiceType == typeof(IGenericAttributeReplaceService)).ShouldBe(1);
            Registrar.ServiceCollection.Count(s => s.ImplementationType == typeof(GenericAttributedManager7)).ShouldBe(0);

            var service = Registrar.ServiceCollection.FirstOrDefault(s => s.ServiceType == typeof(IGenericAttributeReplaceService));
            service.ShouldNotBeNull();
            service.ImplementationType.ShouldBe(typeof(GenericAttributedManager8));
            service.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }
    }
}