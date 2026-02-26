using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection.Proxy;
using Castle.DynamicProxy;

namespace Allegory.Axiom.Castle;

public class AxiomInterceptorCastleAdapter<T>(T interceptor) :
    AsyncInterceptorBase 
    where T : IAxiomInterceptor
{
    protected T Interceptor { get; } = interceptor;

    protected override Task InterceptAsync(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var adapter = new AxiomInterceptorContextCastleAdapter(invocation, proceedInfo, proceed);

        return Interceptor.InterceptAsync(adapter);
    }

    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var adapter = new AxiomInterceptorContextCastleAdapter<TResult>(invocation, proceedInfo, proceed);
        await Interceptor.InterceptAsync(adapter);

        return (TResult) adapter.ReturnValue!;
    }
}