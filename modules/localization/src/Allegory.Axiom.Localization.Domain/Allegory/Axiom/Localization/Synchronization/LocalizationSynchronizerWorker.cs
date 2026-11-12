using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizerWorker(
    ILogger<LocalizationSynchronizerWorker> logger,
    LocalizationSynchronizer synchronizer) :
    BackgroundService
{
    protected ILogger<LocalizationSynchronizerWorker> Logger { get; } = logger;
    protected LocalizationSynchronizer Synchronizer { get; } = synchronizer;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await PushAsync(cancellationToken);
        await Synchronizer.RefreshAsync(cancellationToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
        }
    }

    protected async Task PushAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await Synchronizer.PushAsync(cancellationToken);
                break;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (attempt == maxAttempts)
                {
                    Logger.LogSynchronizationException(exception);
                }
                else
                {
                    var wait = attempt * 3;
                    Logger.LogSynchronizationFailedRetry(wait);
                    await Task.Delay(TimeSpan.FromSeconds(wait), cancellationToken);
                }
            }
        }
    }
}