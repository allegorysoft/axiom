using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Settings.Synchronization;

public class SettingSynchronizerWorker(
    ILogger<SettingSynchronizerWorker> logger,
    SettingDefinitionSynchronizer synchronizer) :
    BackgroundService
{
    protected ILogger<SettingSynchronizerWorker> Logger { get; } = logger;
    public SettingDefinitionSynchronizer Synchronizer { get; } = synchronizer;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SynchronizeAsync(cancellationToken);
    }

    protected async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await Synchronizer.SynchronizeAsync(cancellationToken);
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