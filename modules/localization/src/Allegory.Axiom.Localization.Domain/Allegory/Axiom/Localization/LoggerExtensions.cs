using System;
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
        "Localization synchronization skipped for '{Name}' ({Culture}) because no changes were detected")]
    public static partial void LogSynchronizationSkippedNoChanges(
        this ILogger<LocalizationSynchronizer> logger,
        string name,
        string culture);

    [LoggerMessage(
        LogLevel.Debug,
        "Localization synchronization completed for '{Name}' ({Culture})")]
    public static partial void LogSynchronized(
        this ILogger<LocalizationSynchronizer> logger,
        string name,
        string culture);

    [LoggerMessage(
        LogLevel.Error,
        Message = "Localization synchronization failed")]
    public static partial void LogSynchronizationException(this ILogger logger, Exception exception);

    [LoggerMessage(
        LogLevel.Warning,
        Message = "Localization synchronization failed. Retrying in {Seconds} seconds...")]
    public static partial void LogSynchronizationFailedRetry(this ILogger logger, int seconds);
}