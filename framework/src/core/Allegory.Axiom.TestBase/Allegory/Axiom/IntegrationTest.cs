using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom;

public abstract class IntegrationTest : IAsyncLifetime
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly List<IHost> _hosts = [];

    public IHost Host { get; protected set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        Host = await CreateHostAsync(ConfigureAsync, PostConfigureAsync);
    }

    public virtual async Task<IHost> CreateHostAsync(
        Func<IHostApplicationBuilder, Task>? configureAsync = null,
        Func<IHostApplicationBuilder, Task>? postConfigureAsync = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        if (configureAsync != null)
        {
            await configureAsync(builder);
        }

        await builder.ConfigureApplicationAsync();
        if (postConfigureAsync != null)
        {
            await postConfigureAsync(builder);
        }

        var host = builder.Build();
        await host.InitializeApplicationAsync();

        _hosts.Add(host);
        return host;
    }

    public virtual async Task<IHost> CreateHostAsync(
        Action<IHostApplicationBuilder>? configure = null,
        Action<IHostApplicationBuilder>? postConfigure = null)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        configure?.Invoke(builder);
        await builder.ConfigureApplicationAsync();
        postConfigure?.Invoke(builder);

        var host = builder.Build();
        await host.InitializeApplicationAsync();

        _hosts.Add(host);
        return host;
    }

    public virtual async Task<IServiceProvider> CreateServiceProviderAsync(
        Func<IHostApplicationBuilder, Task>? configureAsync = null,
        Func<IHostApplicationBuilder, Task>? postConfigureAsync = null)
    {
        return (await CreateHostAsync(configureAsync, postConfigureAsync)).Services;
    }

    public virtual async Task<IServiceProvider> CreateServiceProviderAsync(
        Action<IHostApplicationBuilder>? configure = null,
        Action<IHostApplicationBuilder>? postConfigure = null)
    {
        return (await CreateHostAsync(configure, postConfigure)).Services;
    }

    protected virtual Task ConfigureAsync(IHostApplicationBuilder builder) => Task.CompletedTask;

    protected virtual Task PostConfigureAsync(IHostApplicationBuilder builder) => Task.CompletedTask;

    public virtual T Service<T>() where T : notnull
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
            var containers = host.Services.GetServices<TestContainer>();
            var disposes = containers.Select(c => c.DisposeAsync().AsTask()).ToList();
            await Task.WhenAll(disposes);

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