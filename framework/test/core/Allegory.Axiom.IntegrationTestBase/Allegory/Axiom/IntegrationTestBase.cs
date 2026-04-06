using System;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom;

public abstract class IntegrationTestBase : IAsyncLifetime
{
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
}