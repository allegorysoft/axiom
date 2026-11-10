using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Localization;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        LogLevel.Debug,
        "Localization synchronization skipped for '{Name}' because another synchronization is already in progress")]
    public static partial void LogSynchronizationSkipped(this ILogger<LocalizationSynchronizer> logger, string name);

    [LoggerMessage(
        LogLevel.Debug,
        "Localization synchronization skipped for '{Name}' and culture '{Culture}' because there are no changes")]
    public static partial void LogSynchronizationSkippedNoChanges(
        this ILogger<LocalizationSynchronizer> logger,
        string name,
        string culture);

    [LoggerMessage(
        LogLevel.Debug,
        "Localization '{Name}' synchronized for culture '{Culture}'")]
    public static partial void LogSynchronizationCompleted(
        this ILogger<LocalizationSynchronizer> logger,
        string name,
        string culture);
}