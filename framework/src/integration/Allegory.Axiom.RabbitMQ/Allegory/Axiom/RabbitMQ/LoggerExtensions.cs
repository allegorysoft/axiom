using System;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.RabbitMQ;

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        Message = "RabbitMqConnectionFactory failed to gracefully shutdown for {RabbitMqConnection}",
        Level = LogLevel.Error)]
    public static partial void LogFailedGracefulShutdown(
        this ILogger logger,
        Exception exception,
        string rabbitMqConnection);
}