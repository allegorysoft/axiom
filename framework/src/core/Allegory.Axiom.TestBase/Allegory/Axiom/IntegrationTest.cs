using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Allegory.Axiom.Disposables;
using Allegory.Axiom.Hosting;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom;

public abstract class IntegrationTest : IAsyncLifetime
{
    private readonly ConcurrentDictionary<Type, object> _services = new();
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

    public virtual IAsyncDisposable BeginAutoCompletingUnitOfWork(
        IServiceProvider? provider = null,
        UnitOfWorkOptions? options = null)
    {
        provider ??= Host.Services;
        var manager = provider.GetRequiredService<IUnitOfWorkManager>();
        var uow = manager.Begin(options, provider);
        return new AsyncDisposableDelegate<IUnitOfWork>(
            static async s => { await s.TryCompleteAsync(); },
            uow);
    }

    public virtual async Task RunInUnitOfWorkAsync(
        Action<IUnitOfWork> action,
        IServiceProvider? provider = null,
        UnitOfWorkOptions? options = null)
    {
        provider ??= Host.Services;
        var manager = provider.GetRequiredService<IUnitOfWorkManager>();
        await using var uow = manager.Begin(options, provider);
        action(uow);
        
        if (uow.State != UnitOfWorkState.Committed)
        {
            await uow.TryCompleteAsync();
        }
    }

    public virtual async Task RunInUnitOfWorkAsync(
        Func<IUnitOfWork, Task> func,
        IServiceProvider? provider = null,
        UnitOfWorkOptions? options = null)
    {
        provider ??= Host.Services;
        var manager = provider.GetRequiredService<IUnitOfWorkManager>();
        await using var uow = manager.Begin(options, provider);
        await func(uow);

        if (uow.State != UnitOfWorkState.Committed)
        {
            await uow.TryCompleteAsync();
        }
    }

    public virtual T Service<T>() where T : notnull
    {
        return (T) _services.GetOrAdd(typeof(T), t => Host.Services.GetRequiredService(t));
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