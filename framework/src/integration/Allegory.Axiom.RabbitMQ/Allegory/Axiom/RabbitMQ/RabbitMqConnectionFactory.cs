using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqConnectionFactory(
    IOptions<RabbitMqOptions> options)
    : ISingletonService, IDisposable, IAsyncDisposable
{
    protected RabbitMqOptions Options { get; } = options.Value;
    protected ConcurrentDictionary<string, RabbitMqConnection> Connections { get; } = [];

    public virtual async ValueTask<RabbitMqConnection> GetAsync(string name)
    {
        var connection = Connections.GetOrAdd(
            name,
            static (key, options) =>
            {
                return options.TryGetValue(key, out var option)
                    ? new RabbitMqConnection(option)
                    : throw new InvalidOperationException($"There isn't rabbitmq options for {key}");
            },
            Options);

        await connection.TryCreateConnectionAsync();
        return connection;
    }

    public void Dispose()
    {
        foreach (var connection in Connections.Values)
        {
            connection.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in Connections.Values)
        {
            await connection.DisposeAsync();
        }
    }
}