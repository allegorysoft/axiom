using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Allegory.Axiom;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly Dictionary<Type, object> _services = new();

    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    public virtual async ValueTask InitializeAsync()
    {
        var services = new ServiceCollection();
        var registrar = new AssemblyDependencyRegistrar(services);

        await ConfigureAsync(services, registrar);

        ServiceProvider = services.BuildServiceProvider();
    }

    protected virtual ValueTask ConfigureAsync(
        IServiceCollection services,
        AssemblyDependencyRegistrar registrar) => ValueTask.CompletedTask;

    protected T Service<T>() where T : notnull
    {
        if (_services.TryGetValue(typeof(T), out var cached))
        {
            return (T) cached;
        }

        var service = ServiceProvider.GetRequiredService<T>();
        _services.Add(typeof(T), service);
        return service;
    }

    public virtual async ValueTask DisposeAsync()
    {
        switch (ServiceProvider)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }

        _services.Clear();
    }
}