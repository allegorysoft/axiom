using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom;

public abstract class HostedIntegrationTestBase : IAsyncLifetime
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly List<IHost> _hosts = [];

    protected IHost Host { get; set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        Host = await CreateHostAsync((Func<IHostApplicationBuilder, Task>?) ConfigureAsync);
    }

    protected virtual async Task<IHost> CreateHostAsync(
        Func<IHostApplicationBuilder, Task>? configureAsync = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        if (configureAsync != null)
        {
            await configureAsync(builder);
        }
        await builder.ConfigureApplicationAsync();

        var host = builder.Build();
        await host.InitializeApplicationAsync();

        _hosts.Add(host);
        return host;
    }

    protected virtual async Task<IHost> CreateHostAsync(
        Action<IHostApplicationBuilder>? configure = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        configure?.Invoke(builder);
        await builder.ConfigureApplicationAsync();

        var host = builder.Build();
        await host.InitializeApplicationAsync();

        _hosts.Add(host);
        return host;
    }

    protected virtual async Task<IServiceProvider> CreateServiceProviderAsync(
        Func<IHostApplicationBuilder, Task>? configureAsync = null)
    {
        return (await CreateHostAsync((Func<IHostApplicationBuilder, Task>?) configureAsync)).Services;
    }

    protected virtual async Task<IServiceProvider> CreateServiceProviderAsync(
        Action<IHostApplicationBuilder>? configure = null)
    {
        return (await CreateHostAsync(configure)).Services;
    }

    protected virtual Task ConfigureAsync(IHostApplicationBuilder builder) => Task.CompletedTask;

    protected T Service<T>() where T : notnull
    {
        if (_services.TryGetValue(typeof(T), out var cached))
        {
            return (T) cached;
        }

        var service = Host.Services.GetRequiredService<T>();
        _services.Add(typeof(T), service);
        return service;
    }

    public virtual async ValueTask DisposeAsync()
    {
        foreach (var host in _hosts)
        {
            switch (host)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _services.Clear();
    }
}