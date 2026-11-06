using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.DistributedLocking;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizer(
    ILogger<LocalizationSynchronizer> logger,
    ILocalizationProvider localizationProvider,
    IDistributedLock distributedLock,
    LocalizationSerializer serializer) :
    ISingletonService
{
    protected ILogger<LocalizationSynchronizer> Logger { get; } = logger;
    protected ILocalizationProvider LocalizationProvider { get; } = localizationProvider;
    protected IDistributedLock DistributedLock { get; } = distributedLock;
    protected LocalizationSerializer Serializer { get; } = serializer;

    public virtual async Task PushAsync()
    {
        foreach (var context in LocalizationProvider.Contexts)
        {
            await PushAsync(context.Key, context.Value);
        }
    }

    protected virtual async Task PushAsync(string name, LocalizationContext context)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(name);

        if (handle == null)
        {
            Logger.LogSynchronizationSkipped(name);
            return;
        }

        foreach (var (culture, translations) in context.Translations)
        {
            var json = Serializer.Serialize(translations);
            var hash = XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(json));
        }
    }

    //Pull
}