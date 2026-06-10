using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Allegory.Axiom.RabbitMQ;

public class RabbitMqClientFactory(IOptions<RabbitMqOptions> options) : ISingletonService, IAsyncDisposable, IDisposable
{
    protected RabbitMqOptions Options { get; } = options.Value;
    protected Dictionary<string, RabbitMqClient> Clients { get; } = [];
    protected SemaphoreSlim Semaphore { get; } = new(1);

    public virtual async ValueTask<RabbitMqClient> GetAsync(string name)
    {
        if (Clients.TryGetValue(name, out var client))
        {
            return client;
        }

        await Semaphore.WaitAsync();

        try
        {
            if (Clients.TryGetValue(name, out client))
            {
                return client;
            }

            Clients[name] = new RabbitMqClient(await CreateConnectionAsync(name));

            return Clients[name];
        }
        finally
        {
            Semaphore.Release();
        }
    }

    protected virtual async Task<IConnection> CreateConnectionAsync(string name)
    {
        if (!Options.TryGetValue(name, out var option))
        {
            throw new InvalidOperationException($"There isn't rabbitmq options for {name}");
        }

        if (option.Factory != null)
        {
            return await option.Factory();
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(option.Username);
        ArgumentException.ThrowIfNullOrWhiteSpace(option.Password);

        var factory = new ConnectionFactory
        {
            Port = option.Port,
            UserName = option.Username,
            Password = option.Password,
            VirtualHost = option.VirtualHost,
            ClientProvidedName = option.ClientProvidedName ?? Assembly.GetEntryAssembly()?.GetName().Name,
        };

        if (option.Hostnames != null)
        {
            return await factory.CreateConnectionAsync(option.Hostnames);
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(option.Hostname);
        factory.HostName = option.Hostname;
        return await factory.CreateConnectionAsync();
    }

    public void Dispose()
    {
        foreach (var connection in Clients.Values)
        {
            connection.Dispose();
        }

        Semaphore.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in Clients.Values)
        {
            await connection.DisposeAsync();
        }

        Semaphore.Dispose();
    }
}