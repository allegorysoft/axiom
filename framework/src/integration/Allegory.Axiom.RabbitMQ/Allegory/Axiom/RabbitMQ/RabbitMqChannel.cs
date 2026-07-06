using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqChannel(RabbitMqConnection connection) : IDisposable, IAsyncDisposable
{
    public IChannel Channel
    {
        get => IsCreated ? field : throw new InvalidOperationException("RabbitMQ channel is not created");
        protected set;
    } = null!;
    protected RabbitMqConnection Connection { get; } = connection;
    protected bool IsCreated { get; set; }
    protected internal SemaphoreSlim Semaphore { get; } = new(1, 1);
    protected HashSet<string> ConsumerTags { get; } = [];

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
        return await Connection.Connection.CreateChannelAsync(options);
    }

    public virtual async Task BasicConsumeAsync(string queue, bool autoAck, IAsyncBasicConsumer consumer)
    {
        var consumerTag = await Channel.BasicConsumeAsync(
            queue: queue,
            autoAck: false,
            consumer: consumer);
        ConsumerTags.Add(consumerTag);
    }

    protected internal virtual async Task GracefulShutdownAsync()
    {
        foreach (var consumerTag in ConsumerTags)
        {
            await Channel.BasicCancelAsync(consumerTag);
        }
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