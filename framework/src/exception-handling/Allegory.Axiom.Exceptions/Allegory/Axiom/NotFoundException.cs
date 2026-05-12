using System.Net;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom;

public class NotFoundException(
    string? code = null,
    string? message = null,
    LogLevel logLevel = LogLevel.None,
    HttpStatusCode statusCode = HttpStatusCode.NotFound)
    : AxiomException(code, message, logLevel, statusCode) {}