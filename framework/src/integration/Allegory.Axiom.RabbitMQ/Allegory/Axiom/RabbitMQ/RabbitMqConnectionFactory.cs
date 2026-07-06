using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqConnectionFactory(
    ILogger<RabbitMqConnectionFactory> logger,
    IOptions<RabbitMqOptions> options)
    : ISingletonService, IDisposable, IAsyncDisposable
{
    public ILogger<RabbitMqConnectionFactory> Logger { get; } = logger;
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

    protected internal virtual async Task GracefulShutdownAsync()
    {
        await Parallel.ForEachAsync(Connections, async (connection, _) =>
        {
            try
            {
                await connection.Value.GracefulShutdownAsync();
            }
            catch (Exception e)
            {
                Logger.LogFailedGracefulShutdown(e, connection.Key);
            }
        });
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