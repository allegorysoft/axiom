using Allegory.Axiom.DependencyInjection;
using Castle.DynamicProxy;

namespace Allegory.Axiom.Interception;

public class AxiomInterceptorDeterminationCastleAdapter<T>(
    AxiomInterceptorCastleAdapter<T> interceptor) :
    AsyncDeterminationInterceptor(interceptor), ISingletonService
    where T : IAxiomInterceptor {}