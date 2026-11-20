using System;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Settings;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        LogLevel.Debug,
        "Setting synchronization skipped for '{Name}' because another synchronization is already in progress")]
    public static partial void LogSynchronizationSkipped(this ILogger logger, string name);

    [LoggerMessage(
        LogLevel.Debug,
        "Setting synchronization skipped for '{Name}' because no changes were detected")]
    public static partial void LogSynchronizationSkippedNoChanges(this ILogger logger, string name);

    [LoggerMessage(
        LogLevel.Debug,
        "Setting synchronization completed for '{Name}'")]
    public static partial void LogSynchronized(this ILogger logger, string name);

    [LoggerMessage(
        LogLevel.Error,
        "Settings synchronization failed")]
    public static partial void LogSynchronizationException(this ILogger logger, Exception exception);

    [LoggerMessage(
        LogLevel.Warning,
        "Settings synchronization failed. Retrying in {Seconds} seconds...")]
    public static partial void LogSynchronizationFailedRetry(this ILogger logger, int seconds);

    [LoggerMessage(
        LogLevel.Debug,
        "Settings loaded from database")]
    public static partial void LogSettingsLoadedFromDatabase(this ILogger logger);
}