using System.Net;

namespace Allegory.Axiom.Exceptions;

public class BusinessException(
    string? code = null,
    string? message = null,
    HttpStatusCode? httpStatusCode = null)
    : AxiomException(code, message)
{
    public HttpStatusCode? HttpStatusCode { get; init; } = httpStatusCode;
}