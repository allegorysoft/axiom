using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

internal class TransientProductManager : ITransientService {}

internal class ScopedProductManager : IScopedService {}

internal class SingletonProductManager : ISingletonService {}

internal class InheritedProductManager : SingletonProductManager {}

internal class GenericProductManager<T> : ITransientService {}

internal interface ITransientOrderManager : ITransientService {}

internal interface IScopedOrderManager {}

internal interface ISingletonOrderManager : ISingletonService {}

internal class TransientOrderManager : ITransientOrderManager {}

internal class ScopedOrderManager : IScopedOrderManager, IScopedService {}

internal class ExtendedNameSingletonOrderManager : ISingletonOrderManager {}

internal interface IOrderManager : ITransientService {}

internal class OrderNameNotMatchedManager : IOrderManager {}

internal interface IOrderRepository<T> : ITransientService {}

internal class OrderRepository<T> : IOrderRepository<T> {}

internal class OrderRepository : IOrderRepository<int> {}

[Dependency(ServiceLifetime.Transient)]
internal class AttributedTransientProductManager {}

[Dependency(ServiceLifetime.Scoped)]
internal class AttributedScopedProductManager {}

[Dependency(ServiceLifetime.Singleton)]
internal class AttributedSingletonProductManager {}

internal class InheritedAttributedSingletonProductManager : AttributedSingletonProductManager {}

[Dependency(ServiceLifetime.Transient)]
internal class AttributedGenericProductManager<T> {}

internal interface IAttributedTransientOrderManager {}

internal interface IAttributedScopedOrderManager {}

internal interface IAttributedSingletonOrderManager {}

[Dependency(ServiceLifetime.Transient)]
internal class AttributedTransientOrderManager : IAttributedTransientOrderManager {}

[Dependency(ServiceLifetime.Scoped)]
internal class AttributedScopedOrderManager : IAttributedScopedOrderManager {}

[Dependency(ServiceLifetime.Singleton)]
internal class ExtendedNameAttributedSingletonOrderManager : IAttributedSingletonOrderManager {}

internal interface IAttributedOrderManager {}

[Dependency(ServiceLifetime.Transient)]
internal class AttributedOrderNameNotMatchedManager : IAttributedOrderManager {}

internal interface IAttributedOrderRepository<T> {}

[Dependency(ServiceLifetime.Transient)]
internal class AttributedOrderRepository<T> : IAttributedOrderRepository<T> {}

[Dependency(ServiceLifetime.Transient)]
internal class AttributedOrderRepository : IAttributedOrderRepository<int> {}

[Dependency(ServiceLifetime.Transient)]
internal class OverriddenLifetimeProductManager : SingletonProductManager {}

internal interface IKeyedProductManager : ITransientService {}

[Dependency(ServiceKey = 1)]
internal class KeyedProductManager : IKeyedProductManager {}

[Dependency(AutoRegister = false)]
internal class SkipRegisterForThisClass : ITransientService {}

[Dependency(ServiceLifetime.Transient, Strategy = RegistrationStrategy.TryAdd)]
internal class TryAddAttributedTransientOrderManager : IAttributedTransientOrderManager {}

internal interface ICustomerManager : ITransientService {}

internal class CustomerManager : ICustomerManager {}

[Dependency(Strategy = RegistrationStrategy.Replace)]
internal class ReplacedCustomerManager : ICustomerManager {}

internal interface IFooManager : ITransientService {}

[Dependency<IFooManager>]
internal class GenericAttributedManager : IFooManager {}

internal interface IZooManager {}

internal interface IHooManager {}

[Dependency<IZooManager>(ServiceLifetime.Transient)]
[Dependency<IHooManager>(ServiceLifetime.Scoped)]
internal class GenericAttributedManager2 : IZooManager, IHooManager, ISingletonService {}

internal interface IGooManager {}

[Dependency<IGooManager>(ServiceLifetime.Scoped)]
internal class GenericAttributedManager3 : IGooManager {}

internal interface IGenericAttributedKeyedService : ITransientService {}

[Dependency<IGenericAttributedKeyedService>(ServiceKey = 1)]
internal class GenericAttributedManager4 : IGenericAttributedKeyedService {}

internal interface IGenericAttributeTryAddService : ITransientService {}

[Dependency<IGenericAttributeTryAddService>]
internal class GenericAttributedManager5 : IGenericAttributeTryAddService {}

[Dependency<IGenericAttributeTryAddService>(Strategy = RegistrationStrategy.TryAdd)]
internal class GenericAttributedManager6 : IGenericAttributeTryAddService {}

internal interface IGenericAttributeReplaceService : ISingletonService {}

[Dependency<IGenericAttributeReplaceService>]
internal class GenericAttributedManager7 : IGenericAttributeReplaceService {}

[Dependency<IGenericAttributeReplaceService>(Strategy = RegistrationStrategy.Replace)]
internal class GenericAttributedManager8 : IGenericAttributeReplaceService {}