using System;
using System.Threading.Tasks;
using Allegory.Axiom.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Allegory.Axiom;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IHost Host { get; set; } = null!;
    protected IServiceProvider ServiceProvider => Host.Services;

    public virtual async ValueTask InitializeAsync()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        await builder.ConfigureApplicationAsync();
        Host = builder.Build();
        await Host.InitializeApplicationAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        Host.Dispose();
        return ValueTask.CompletedTask;
    }
}