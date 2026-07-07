using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventProcessorCompletionService(
    DistributedEventProcessor eventProcessor,
    ILogger<DistributedEventProcessorCompletionService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Waiting for distributed events to complete...");
            await eventProcessor.WaitForCompletionAsync(cancellationToken);
            logger.LogInformation("Distributed events completed");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Graceful shutdown for distributed events failed");
        }
    }
}