using System.Threading.Tasks;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public interface IAxiomInterceptor
{
    Task InterceptAsync(IAxiomInterceptorContext context);
}