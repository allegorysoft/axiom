using System.Reflection;
using System.Threading.Tasks;

namespace Allegory.Axiom.Interception;

public interface IAxiomInterceptorContext
{
    MethodInfo Method { get; }
    object? Target { get; }
    object?[] Arguments { get; }
    object? ReturnValue { get; set; }
    Task ProceedAsync();
}