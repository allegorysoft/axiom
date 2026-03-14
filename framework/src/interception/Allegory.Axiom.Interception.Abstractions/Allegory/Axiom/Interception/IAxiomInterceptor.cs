using System.Threading.Tasks;

namespace Allegory.Axiom.Interception;

public interface IAxiomInterceptor
{
    Task InterceptAsync(IAxiomInterceptorContext context);
}