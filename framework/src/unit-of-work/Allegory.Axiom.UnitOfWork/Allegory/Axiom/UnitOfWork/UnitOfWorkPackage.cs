using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.Interception;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.UnitOfWork;

internal sealed class UnitOfWorkPackage : IConfigureApplication
{
    public static ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.AddInterceptor<UnitOfWorkInterceptor>(AddUnitOfWorkInterceptor);

        return ValueTask.CompletedTask;
    }

    private static bool AddUnitOfWorkInterceptor(Type type)
    {
        if (IsUnitOfWorkAttributeDefined(type) || typeof(IUnitOfWorkScope).IsAssignableFrom(type))
        {
            return true;
        }

        return type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Any(IsUnitOfWorkAttributeDefined);
    }

    private static bool IsUnitOfWorkAttributeDefined(MemberInfo memberInfo)
    {
        return memberInfo.IsDefined(typeof(UnitOfWorkAttribute));
    }
}