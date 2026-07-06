using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.RabbitMQ;

internal sealed class RabbitMqConsumerShutdownService(RabbitMqConnectionFactory factory) : IHostedLifecycleService
{
    public RabbitMqConnectionFactory Factory { get; } = factory;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        await Factory.GracefulShutdownAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}