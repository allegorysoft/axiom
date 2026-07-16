using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.EventBus.Distributed;

public class DistributedEventProcessorCompletionService(
    DistributedEventProcessor eventProcessor,
    ILogger<DistributedEventProcessorCompletionService> logger)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogWaitingForPendingEvents(eventProcessor.PendingProcesses);
            await eventProcessor.WaitForCompletionAsync(cancellationToken);
            logger.LogPendingEventsCompleted();
        }
        catch (OperationCanceledException)
        {
            logger.LogDrainCancelled();
        }
        catch (Exception e)
        {
            logger.LogGracefulShutdownFailed(e);
        }
    }
}