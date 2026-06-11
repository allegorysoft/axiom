using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqChannel(RabbitMqClient client) : IDisposable, IAsyncDisposable
{
    public IChannel Channel
    {
        get => IsCreated ? field : throw new InvalidOperationException("RabbitMQ channel is not created");
        protected set;
    } = null!;
    protected RabbitMqClient Client { get; } = client;
    protected bool IsCreated { get; set; }
    protected internal SemaphoreSlim Semaphore { get; } = new(1, 1);

    protected internal virtual async Task TryCreateChannelAsync(CreateChannelOptions? options = null)
    {
        if (IsCreated)
        {
            return;
        }

        await Semaphore.WaitAsync();

        try
        {
            if (IsCreated)
            {
                return;
            }

            Channel = await CreateChannelAsync(options);
            IsCreated = true;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    protected virtual async Task<IChannel> CreateChannelAsync(CreateChannelOptions? options = null)
    {
        return await Client.Connection.CreateChannelAsync(options);
    }

    public virtual void Dispose()
    {
        if (IsCreated)
        {
            Channel.Dispose();
        }

        Semaphore.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (IsCreated)
        {
            await Channel.DisposeAsync();
        }

        Semaphore.Dispose();
    }
}