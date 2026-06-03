using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EventBus;

internal sealed class EventBusPackage : IConfigureApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        RegisterHandlers(builder);

        return Task.CompletedTask;
    }

    private static void RegisterHandlers(IHostApplicationBuilder builder)
    {
        var assemblies = builder.GetAxiomApplication().Assemblies;

        var localHandlers = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ILocalEventHandler<>))
                .Select(i => (EventType: i.GetGenericArguments()[0], HandlerType: t)))
            .GroupBy(x => x.EventType, x => x.HandlerType)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var localHandler in localHandlers.Values.SelectMany(t => t))
        {
            builder.Services.Add(ServiceDescriptor.Singleton(localHandler, localHandler));
        }

        builder.Services.Configure<LocalEventBusOptions>(options =>
        {
            options.Handlers = localHandlers;
        });
    }
}