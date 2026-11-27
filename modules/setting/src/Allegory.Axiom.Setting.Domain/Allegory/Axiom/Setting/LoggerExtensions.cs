using System;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Setting;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        LogLevel.Debug,
        "Setting definition synchronization skipped for '{Name}' because another synchronization is already in progress")]
    public static partial void LogSynchronizationSkipped(this ILogger logger, string name);

    [LoggerMessage(
        LogLevel.Debug,
        "Setting definition synchronization skipped for '{Name}' because no changes were detected")]
    public static partial void LogSynchronizationSkippedNoChanges(this ILogger logger, string name);

    [LoggerMessage(
        LogLevel.Debug,
        "Setting definition synchronization completed for '{Name}'")]
    public static partial void LogSynchronized(this ILogger logger, string name);

    [LoggerMessage(
        LogLevel.Error,
        "Setting definitions synchronization failed")]
    public static partial void LogSynchronizationException(this ILogger logger, Exception exception);

    [LoggerMessage(
        LogLevel.Warning,
        "Setting definitions synchronization failed. Retrying in {Seconds} seconds...")]
    public static partial void LogSynchronizationFailedRetry(this ILogger logger, int seconds, Exception? exception);

    [LoggerMessage(
        LogLevel.Debug,
        "Setting definitions loaded from database")]
    public static partial void LogSettingsLoadedFromDatabase(this ILogger logger);
}