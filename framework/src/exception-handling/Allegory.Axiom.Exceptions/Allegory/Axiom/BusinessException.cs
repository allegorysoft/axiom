using System.Net;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom;

public class BusinessException(
    string? code = null,
    string? message = null,
    LogLevel logLevel = LogLevel.None,
    HttpStatusCode statusCode = HttpStatusCode.Conflict)
    : AxiomException(code, message, logLevel, statusCode) {}