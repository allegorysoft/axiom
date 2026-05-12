namespace Allegory.Axiom.Exceptions;

public class AuthorizationException(
    string? code = null,
    string? message = null)
    : AxiomException(code, message) {}