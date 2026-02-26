using System;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection.Proxy;
using Castle.DynamicProxy;

namespace Allegory.Axiom.Castle;

public sealed class AxiomInterceptorContextCastleAdapter(
    IInvocation invocation,
    IInvocationProceedInfo proceedInfo,
    Func<IInvocation, IInvocationProceedInfo, Task> proceed) :
    IAxiomInterceptorContext
{
    public IInvocation Invocation { get; } = invocation;
    public IInvocationProceedInfo ProceedInfo { get; } = proceedInfo;
    private Func<IInvocation, IInvocationProceedInfo, Task> Proceed { get; } = proceed;

    public MethodInfo Method => Invocation.Method;

    public object?[] Arguments => Invocation.Arguments;

    public object? ReturnValue
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public object? Target => Invocation.InvocationTarget;

    public Task ProceedAsync() => Proceed(Invocation, ProceedInfo);
}

public sealed class AxiomInterceptorContextCastleAdapter<TResult>(
    IInvocation invocation,
    IInvocationProceedInfo proceedInfo,
    Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed) :
    IAxiomInterceptorContext
{
    public IInvocation Invocation { get; } = invocation;
    public IInvocationProceedInfo ProceedInfo { get; } = proceedInfo;
    private Func<IInvocation, IInvocationProceedInfo, Task<TResult>> Proceed { get; } = proceed;

    public MethodInfo Method => Invocation.Method;

    public object?[] Arguments => Invocation.Arguments;

    public object? ReturnValue { get; set; }

    public object? Target => Invocation.InvocationTarget;

    public async Task ProceedAsync()
    {
        ReturnValue = await Proceed(Invocation, ProceedInfo);
    }
}