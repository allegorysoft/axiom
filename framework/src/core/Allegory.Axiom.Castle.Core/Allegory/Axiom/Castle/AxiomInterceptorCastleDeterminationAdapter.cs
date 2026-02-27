using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DependencyInjection.Proxy;
using Castle.DynamicProxy;

namespace Allegory.Axiom.Castle;

public class AxiomInterceptorCastleDeterminationAdapter<T>(
    AxiomInterceptorCastleAdapter<T> interceptor) :
    AsyncDeterminationInterceptor(interceptor), ISingletonService
    where T : IAxiomInterceptor {}