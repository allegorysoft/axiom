using System;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.AspNetCore.ExceptionHandling;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Message = "Exception occurred. Code: {ExceptionCode}")]
    public static partial void LogException(
        ILogger logger,
        LogLevel level,
        Exception exception,
        string? exceptionCode);
}