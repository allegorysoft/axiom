using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed.Inbox;

public class InboxWorker(
    ILogger<InboxWorker> logger,
    IOptions<DistributedEventBusOptions> options,
    IInboxStore store)
    : BackgroundService
{
    protected ILogger<InboxWorker> Logger { get; } = logger;
    protected InboxOptions Options { get; } = options.Value.Inbox;
    protected IInboxStore Store { get; } = store;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!Options.IsWorkerEnabled)
        {
            Logger.LogWarning("Inbox worker is disabled");
            return;
        }

        //TODO: Implement
    }
}