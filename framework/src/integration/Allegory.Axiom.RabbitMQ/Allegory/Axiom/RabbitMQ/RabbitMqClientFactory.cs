using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqClientFactory(
    IOptions<RabbitMqOptions> options)
    : ISingletonService, IDisposable, IAsyncDisposable
{
    protected RabbitMqOptions Options { get; } = options.Value;
    protected ConcurrentDictionary<string, RabbitMqClient> Clients { get; } = [];

    public virtual async ValueTask<RabbitMqClient> GetAsync(string name)
    {
        var client = Clients.GetOrAdd(
            name,
            static (key, options) =>
            {
                return options.TryGetValue(key, out var option)
                    ? new RabbitMqClient(option)
                    : throw new InvalidOperationException($"There isn't rabbitmq options for {key}");
            },
            Options);

        await client.TryCreateConnectionAsync();
        return client;
    }

    public void Dispose()
    {
        foreach (var client in Clients.Values)
        {
            client.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in Clients.Values)
        {
            await client.DisposeAsync();
        }
    }
}