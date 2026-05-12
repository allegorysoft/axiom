using System.Net;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom;

public class AuthorizationException(
    string? code = null,
    string? message = null,
    LogLevel logLevel = LogLevel.Warning,
    HttpStatusCode statusCode = HttpStatusCode.Forbidden)
    : AxiomException(code, message, logLevel, statusCode) {}