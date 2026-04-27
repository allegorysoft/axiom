namespace Allegory.Axiom.Threading;

public class AsyncLocalContext<TContext>(TContext? context)
{
    public TContext? Context { get; set; } = context;
}