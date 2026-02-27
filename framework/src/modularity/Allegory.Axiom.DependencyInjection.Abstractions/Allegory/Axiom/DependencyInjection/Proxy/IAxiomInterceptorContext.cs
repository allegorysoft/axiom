using System.Reflection;
using System.Threading.Tasks;

namespace Allegory.Axiom.DependencyInjection.Proxy;

public interface IAxiomInterceptorContext
{
    MethodInfo Method { get; }
    object? Target { get; }
    object?[] Arguments { get; }
    object? ReturnValue { get; set; }
    //IServiceProvider
    Task ProceedAsync();
}