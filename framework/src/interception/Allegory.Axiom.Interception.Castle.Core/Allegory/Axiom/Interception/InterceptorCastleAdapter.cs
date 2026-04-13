using System;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Castle.DynamicProxy;

namespace Allegory.Axiom.Interception;

public class InterceptorCastleAdapter<T>(T interceptor) :
    AsyncInterceptorBase, ISingletonService
    where T : IInterceptor
{
    protected T Interceptor { get; } = interceptor;

    protected override Task InterceptAsync(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var adapter = new InterceptorContextCastleAdapter(invocation, proceedInfo, proceed);
        return Interceptor.InterceptAsync(adapter);
    }

    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var adapter = new InterceptorContextCastleAdapter<TResult>(invocation, proceedInfo, proceed);
        await Interceptor.InterceptAsync(adapter);

        return (TResult) adapter.ReturnValue!;
    }
}

public class DeterminationInterceptorCastleAdapter<T>(
    InterceptorCastleAdapter<T> interceptor) :
    AsyncDeterminationInterceptor(interceptor), ISingletonService
    where T : IInterceptor {}