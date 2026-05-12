using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom;

public abstract class AxiomException(
    string? code = null,
    string? message = null,
    LogLevel logLevel = LogLevel.Error,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : Exception
{
    public virtual string? Code { get; init; } = code;
    public override string Message { get; } = message ?? string.Empty;
    public virtual LogLevel LogLevel { get; } = logLevel;
    public HttpStatusCode HttpStatusCode { get; } = statusCode;
}