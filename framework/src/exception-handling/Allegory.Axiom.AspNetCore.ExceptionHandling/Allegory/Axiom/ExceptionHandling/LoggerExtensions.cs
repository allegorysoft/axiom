using System;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.ExceptionHandling;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Message = "AxiomException occurred. Code: {ExceptionCode}, StatusCode: {HttpStatusCode}")]
    public static partial void LogException(
        ILogger logger,
        LogLevel level,
        Exception exception,
        string? exceptionCode,
        System.Net.HttpStatusCode httpStatusCode);
}