using System;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.EventBus;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        LogLevel.Debug,
        "Waiting for {Count} pending distributed event(s) to complete...")]
    public static partial void LogWaitingForPendingEvents(this ILogger logger, int count);

    [LoggerMessage(
        LogLevel.Debug,
        "All distributed events completed, shutdown proceeding")]
    public static partial void LogPendingEventsCompleted(this ILogger logger);

    [LoggerMessage(
        LogLevel.Warning,
        "Distributed event drain cancelled before completion (shutdown timeout exceeded)")]
    public static partial void LogDrainCancelled(this ILogger logger);

    [LoggerMessage(
        LogLevel.Error,
        "Graceful shutdown for distributed events failed")]
    public static partial void LogGracefulShutdownFailed(this ILogger logger, Exception exception);
}