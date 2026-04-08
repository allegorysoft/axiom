using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Testing.Platform.Services;
using Xunit;

namespace Allegory.Axiom;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly Dictionary<Type, object> _services = new();

    protected HostApplicationBuilder Builder { get; } = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
    protected IHost Host { get; set; } = null!;
    protected IServiceProvider ServiceProvider => Host.Services;

    public virtual async ValueTask InitializeAsync()
    {
        await Builder.ConfigureApplicationAsync();
        Host = Builder.Build();
        await Host.InitializeApplicationAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        Host.Dispose();
        return ValueTask.CompletedTask;
    }

    protected T Service<T>() where T : notnull
    {
        if (_services.TryGetValue(typeof(T), out var svc))
        {
            return (T) svc;
        }

        var service = ServiceProvider.GetRequiredService<T>();
        _services.Add(typeof(T), service);
        return service;
    }
}