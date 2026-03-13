using Castle.DynamicProxy;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public class AxiomInterceptorCastleDeterminationAdapter<T>(
    AxiomInterceptorCastleAdapter<T> interceptor) :
    AsyncDeterminationInterceptor(interceptor), ISingletonService
    where T : IAxiomInterceptor {}