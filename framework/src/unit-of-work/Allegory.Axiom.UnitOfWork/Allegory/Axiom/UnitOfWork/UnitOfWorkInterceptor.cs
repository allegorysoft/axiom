using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Interception;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkInterceptor(IUnitOfWorkManager unitOfWorkManager) : IAxiomInterceptor, ISingletonService
{
    protected static readonly string[] ReadPrefixes =
        ["Get", "Find", "Search", "List", "Count", "Exists", "Check", "Is", "Has"];

    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected internal ConcurrentDictionary<MethodInfo, UnitOfWorkDescriptor> MethodInfoCache { get; } = new();

    public virtual async Task InterceptAsync(IAxiomInterceptorContext context)
    {
        var uowDescriptor = MethodInfoCache.GetOrAdd(context.Method, GetDescriptor);

        if (!uowDescriptor.IsEnabled)
        {
            await context.ProceedAsync();
            return;
        }

        await using var uow = UnitOfWorkManager.Begin(uowDescriptor.Options);
        await context.ProceedAsync();
        await uow.CompleteAsync();
    }

    protected virtual UnitOfWorkDescriptor GetDescriptor(MethodInfo methodInfo)
    {
        var attribute = methodInfo.GetCustomAttribute<UnitOfWorkAttribute>();

        return attribute == null
            ? ResolveFromDeclaringType(methodInfo)
            : BuildDescriptorFromAttribute(attribute, methodInfo);
    }

    protected virtual UnitOfWorkDescriptor ResolveFromDeclaringType(MethodInfo methodInfo)
    {
        var type = methodInfo.DeclaringType;

        if (type == null)
        {
            return new UnitOfWorkDescriptor(false);
        }

        var attribute = type.GetCustomAttribute<UnitOfWorkAttribute>();

        if (attribute != null)
        {
            return BuildDescriptorFromAttribute(attribute, methodInfo);
        }

        if (typeof(IUnitOfWorkScope).IsAssignableFrom(type))
        {
            var transactionBehavior = TryGetDefaultBehaviour(methodInfo);

            return transactionBehavior == null
                ? new UnitOfWorkDescriptor(true)
                : new UnitOfWorkDescriptor(true, new UnitOfWorkOptions(transactionBehavior));
        }

        return new UnitOfWorkDescriptor(false);
    }

    protected virtual UnitOfWorkDescriptor BuildDescriptorFromAttribute(
        UnitOfWorkAttribute attribute,
        MethodInfo methodInfo)
    {
        if (!attribute.IsEnabled)
        {
            return new UnitOfWorkDescriptor(false);
        }

        UnitOfWorkOptions? options = null;
        var behavior = attribute.TransactionBehavior ?? TryGetDefaultBehaviour(methodInfo);

        if (behavior.HasValue ||
            attribute.Timeout.HasValue ||
            attribute.IsolationLevel.HasValue)
        {
            options = new UnitOfWorkOptions(behavior, attribute.IsolationLevel, attribute.Timeout);
        }

        return new UnitOfWorkDescriptor(true, options);
    }

    protected virtual UnitOfWorkTransactionBehavior? TryGetDefaultBehaviour(MethodInfo methodInfo)
    {
        return ReadPrefixes.Any(p => methodInfo.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            ? UnitOfWorkTransactionBehavior.Suppress
            : null;
    }
}