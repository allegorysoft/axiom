using System.Threading.Tasks;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public interface IAxiomInterceptor : ISingletonService
{
    Task InterceptAsync(IAxiomInterceptorContext context);
}