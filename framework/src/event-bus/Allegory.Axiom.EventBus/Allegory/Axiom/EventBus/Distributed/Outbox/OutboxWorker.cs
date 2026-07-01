using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed.Outbox;

public class OutboxWorker(
    ILogger<OutboxWorker> logger,
    IOptions<DistributedEventBusOptions> options,
    IOutboxStore store)
    : BackgroundService
{
    protected ILogger<OutboxWorker> Logger { get; } = logger;
    protected OutboxOptions Options { get; } = options.Value.Outbox;
    protected IOutboxStore Store { get; } = store;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!Options.IsWorkerEnabled)
        {
            Logger.LogWarning("Outbox worker is disabled");
            return;
        }

        //TODO: Implement
    }
}