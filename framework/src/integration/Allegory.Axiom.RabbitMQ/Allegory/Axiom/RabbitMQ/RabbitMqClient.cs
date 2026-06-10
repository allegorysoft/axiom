using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqClient(IConnection connection) : IDisposable, IAsyncDisposable
{
    public IConnection Connection { get; } = connection;
    protected SemaphoreSlim Semaphore { get; } = new(1);
    protected Dictionary<string, IChannel> Channels { get; } = [];

    public virtual async ValueTask<IChannel> GetChannelAsync(string name, CreateChannelOptions? options = null)
    {
        if (Channels.TryGetValue(name, out var connection))
        {
            return connection;
        }

        await Semaphore.WaitAsync();

        try
        {
            if (Channels.TryGetValue(name, out connection))
            {
                return connection;
            }

            Channels[name] = await CreateChannelAsync(options);

            return Channels[name];
        }
        finally
        {
            Semaphore.Release();
        }
    }

    protected virtual async Task<IChannel> CreateChannelAsync(CreateChannelOptions? options = null)
    {
        return await Connection.CreateChannelAsync();
    }

    public void Dispose()
    {
        foreach (var channel in Channels.Values)
        {
            channel.Dispose();
        }

        Connection.Dispose();
        Semaphore.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var channel in Channels.Values)
        {
            await channel.DisposeAsync();
        }

        await Connection.DisposeAsync();
        Semaphore.Dispose();
    }
}