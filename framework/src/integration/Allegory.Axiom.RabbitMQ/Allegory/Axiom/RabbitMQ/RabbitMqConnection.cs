using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqConnection(RabbitMqOption option) : IDisposable, IAsyncDisposable
{
    public IConnection Connection
    {
        get => IsCreated ? field : throw new InvalidOperationException("RabbitMQ connection is not created");
        protected set;
    } = null!;
    protected RabbitMqOption Option { get; } = option;
    protected bool IsCreated { get; set; }
    protected SemaphoreSlim Semaphore { get; } = new(1, 1);
    protected ConcurrentDictionary<string, RabbitMqChannel> Channels { get; } = [];

    public virtual async ValueTask<RabbitMqChannel> GetChannelAsync(
        string name,
        CreateChannelOptions? options = null)
    {
        var channel = Channels.GetOrAdd(name, static (_, instance) => new RabbitMqChannel(instance), this);
        await channel.TryCreateChannelAsync(options);
        return channel;
    }

    public virtual async ValueTask<RabbitMqChannelLease> RentChannelAsync(
        string name,
        CreateChannelOptions? options = null)
    {
        var channel = await GetChannelAsync(name, options);
        await channel.Semaphore.WaitAsync();
        return new RabbitMqChannelLease(channel);
    }

    protected internal virtual async Task TryCreateConnectionAsync()
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

            Connection = await CreateConnectionAsync();
            IsCreated = true;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    protected virtual async Task<IConnection> CreateConnectionAsync()
    {
        if (Option.Factory != null)
        {
            return await Option.Factory(Option);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(Option.Username);
        ArgumentException.ThrowIfNullOrWhiteSpace(Option.Password);

        var factory = new ConnectionFactory
        {
            Port = Option.Port,
            UserName = Option.Username,
            Password = Option.Password,
            VirtualHost = Option.VirtualHost,
            ClientProvidedName = Option.ClientProvidedName ?? Assembly.GetEntryAssembly()?.GetName().Name,
        };

        if (Option.Hostnames != null)
        {
            return await factory.CreateConnectionAsync(Option.Hostnames);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(Option.Hostname);
        factory.HostName = Option.Hostname;
        return await factory.CreateConnectionAsync();
    }

    public virtual void Dispose()
    {
        foreach (var channel in Channels.Values)
        {
            channel.Dispose();
        }

        if (IsCreated)
        {
            Connection.Dispose();
        }

        Semaphore.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        foreach (var channel in Channels.Values)
        {
            await channel.DisposeAsync();
        }

        if (IsCreated)
        {
            await Connection.DisposeAsync();
        }

        Semaphore.Dispose();
    }
}