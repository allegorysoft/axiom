using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizerWorker(LocalizationSynchronizer synchronizer) : BackgroundService
{
    public LocalizationSynchronizer Synchronizer { get; } = synchronizer;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Synchronizer.PushAsync(); // Re-try policy

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
        }
    }
}