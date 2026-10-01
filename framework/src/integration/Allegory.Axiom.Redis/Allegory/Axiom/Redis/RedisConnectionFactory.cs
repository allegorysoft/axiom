using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Allegory.Axiom.Redis;

public class RedisConnectionFactory(IOptions<RedisOptions> options) : ISingletonService, IDisposable, IAsyncDisposable
{
    protected RedisOptions Options { get; } = options.Value;
    protected ConcurrentDictionary<string, IConnectionMultiplexer> Connections { get; } = [];

    // A single semaphore is sufficient since connection creation happens only once per name.
    protected SemaphoreSlim Semaphore { get; } = new(1, 1);

    public virtual async ValueTask<IConnectionMultiplexer> GetAsync(string name)
    {
        if (Connections.TryGetValue(name, out var connection))
        {
            return connection;
        }

        return await GetConnectionAsync(name);
    }

    public virtual IConnectionMultiplexer Get(string name)
    {
        return Connections.TryGetValue(name, out var connection) ? connection : GetConnection(name);
    }

    protected virtual async Task<IConnectionMultiplexer> GetConnectionAsync(string name)
    {
        await Semaphore.WaitAsync();

        try
        {
            if (Connections.TryGetValue(name, out var connection))
            {
                return connection;
            }

            var option = Options[name];

            if (option.ConfigurationOptions != null)
            {
                connection = await ConnectionMultiplexer.ConnectAsync(option.ConfigurationOptions);
            }
            else
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(option.Configuration);
                connection = option.ConfigureOptionsAction == null
                    ? await ConnectionMultiplexer.ConnectAsync(option.Configuration)
                    : await ConnectionMultiplexer.ConnectAsync(option.Configuration, option.ConfigureOptionsAction);
            }

            Connections[name] = connection;
            return connection;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    protected virtual IConnectionMultiplexer GetConnection(string name)
    {
        Semaphore.Wait();

        try
        {
            if (Connections.TryGetValue(name, out var connection))
            {
                return connection;
            }

            var option = Options[name];

            if (option.ConfigurationOptions != null)
            {
                connection = ConnectionMultiplexer.Connect(option.ConfigurationOptions);
            }
            else
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(option.Configuration);
                connection = option.ConfigureOptionsAction == null
                    ? ConnectionMultiplexer.Connect(option.Configuration)
                    : ConnectionMultiplexer.Connect(
                        option.Configuration,
                        option.ConfigureOptionsAction);
            }

            Connections[name] = connection;
            return connection;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public virtual void Dispose()
    {
        foreach (var connection in Connections.Values)
        {
            connection.Dispose();
        }

        Semaphore.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        foreach (var connection in Connections.Values)
        {
            await connection.DisposeAsync();
        }

        Semaphore.Dispose();
    }
}