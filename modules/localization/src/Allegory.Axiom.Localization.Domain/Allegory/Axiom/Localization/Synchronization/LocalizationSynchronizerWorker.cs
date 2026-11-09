using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Caching;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizerWorker(
    ILocalizationProvider localizationProvider,
    ILocalizationRecordRepository localizationRepository,
    ICache cache,
    IUnitOfWorkManager unitOfWorkManager) :
    BackgroundService
{
    public ILocalizationProvider LocalizationProvider { get; } = localizationProvider;
    public ILocalizationRecordRepository LocalizationRepository { get; } = localizationRepository;
    public ICache Cache { get; } = cache;
    public IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Sync first?
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
        }

    }

    protected virtual async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
    }
}