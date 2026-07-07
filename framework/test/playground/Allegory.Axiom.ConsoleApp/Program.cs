using System;
using System.Threading.Tasks;
using Allegory.Axiom.EventBus;
using Allegory.Axiom.EventBus.Distributed;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
await builder.ConfigureApplicationAsync();
var app = builder.Build();
await app.InitializeApplicationAsync();
await DoAsync();
await app.RunAsync();

return;

async Task DoAsync()
{
    var eventBus = app.Services.GetRequiredService<IDistributedEventBus>();

    await eventBus.PublishAsync(new Event1(Guid.NewGuid()));
}

public record Event1(Guid Id);

public class EventHandler1 : IDistributedEventHandler<Event1>
{
    private readonly IHostApplicationLifetime _lifetime;

    public EventHandler1(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }
    public async Task HandleAsync(Event1 payload, EventContext context)
    {
        await Task.Delay(3_000);
    }
}