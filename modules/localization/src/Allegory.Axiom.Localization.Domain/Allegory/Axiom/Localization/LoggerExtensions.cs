using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.Localization;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        LogLevel.Debug,
        "Localization synchronization skipped for '{Name}' because another synchronization is already in progress")]
    public static partial void LogSynchronizationSkipped(this ILogger<LocalizationSynchronizer> logger, string name);
}