using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.EventBus.Local;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.EventBus;

internal sealed class EventBusPackage : IConfigureApplication, IPostConfigureApplication, IInitializeApplication
{
    public static Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        RegisterEvents(builder);

        // Configure in Inbox/Outbox package
        // builder.Services.Configure<DistributedEventBusOptions>(options =>
        // {
        //     options.Inbox.IsWorkerEnabled = true;
        //     options.Inbox.UseFor ??= static _ => true;
        //
        //     options.Outbox.IsWorkerEnabled = true;
        //     options.Outbox.UseFor ??= static _ => true;
        // });
        //
        // builder.Services.AddHostedService<InboxWorker>();
        // builder.Services.AddHostedService<OutboxWorker>();

        return Task.CompletedTask;
    }

    private static void RegisterEvents(IHostApplicationBuilder builder)
    {
        var targetAssembly = typeof(IEventHandler<>).Assembly;

        var assemblies = builder.GetAxiomApplication().Assemblies
            .Where(a => a.GetReferencedAssemblies()
                .Any(r => r.FullName == targetAssembly.FullName) || a == targetAssembly)
            .ToImmutableArray();

        RegisterLocalEvents(builder, assemblies);
        RegisterDistributedEvents(builder, assemblies);
    }

    private static void RegisterLocalEvents(
        IHostApplicationBuilder builder,
        ImmutableArray<Assembly> assemblies)
    {
        var events = GetEvents<ILocalEventHandler>(assemblies);

        foreach (var handler in events.Values.SelectMany(t => t).Distinct())
        {
            builder.Services.TryAdd(ServiceDescriptor.Singleton(handler, handler));
        }

        builder.Services.Configure<LocalEventBusOptions>(options =>
        {
            options.Events = events;
        });
    }

    private static void RegisterDistributedEvents(
        IHostApplicationBuilder builder,
        ImmutableArray<Assembly> assemblies)
    {
        var events = ImmutableArray.CreateBuilder<DistributedEventDescriptor>();

        foreach (var (eventType, handlers) in GetEvents<IDistributedEventHandler>(assemblies))
        {
            var descriptor = new DistributedEventDescriptor
            {
                Type = eventType,
                Name = eventType.FullName ?? throw new InvalidOperationException("Event name cannot be null"),
                Topic = TopicNameAttribute.Get(eventType),
                Handlers = handlers,
            };

            events.Add(descriptor);
        }

        foreach (var handler in events.SelectMany(t => t.Handlers).Distinct())
        {
            builder.Services.TryAdd(ServiceDescriptor.Singleton(handler, handler));
        }

        builder.Services.Configure<DistributedEventBusOptions>(options =>
        {
            options.Events = events.ToImmutable();
        });
    }

    private static FrozenDictionary<Type, ImmutableArray<Type>> GetEvents<T>(ImmutableArray<Assembly> assemblies)
    {
        var eventType = typeof(T);
        var genericEventType = eventType switch
        {
            _ when eventType == typeof(ILocalEventHandler) => typeof(ILocalEventHandler<>),
            _ when eventType == typeof(IDistributedEventHandler) => typeof(IDistributedEventHandler<>),
            _ => throw new ArgumentException($"Unsupported event handler type: {eventType}", nameof(eventType))
        };

        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is {IsClass: true, IsAbstract: false} && eventType.IsAssignableFrom(t))
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericEventType)
                .Select(i => (EventType: i.GetGenericArguments()[0], HandlerType: t)))
            .GroupBy(x => x.EventType, x => x.HandlerType)
            .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());
    }

    public static Task PostConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<DistributedEventBusOptions>(
            builder.Configuration.GetSection("Axiom:EventBus:Distributed"));

        return Task.CompletedTask;
    }

    public static async Task InitializeAsync(IHost host)
    {
        await host.Services
            .GetRequiredService<IDistributedEventBus>()
            .InitializeAsync();
    }
}