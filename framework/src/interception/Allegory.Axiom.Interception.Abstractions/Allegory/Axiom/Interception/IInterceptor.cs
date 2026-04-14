using System.Threading.Tasks;

namespace Allegory.Axiom.Interception;

public interface IInterceptor
{
    Task InterceptAsync(IInterceptorContext context);
}