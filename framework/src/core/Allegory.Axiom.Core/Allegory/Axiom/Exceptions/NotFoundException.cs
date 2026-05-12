namespace Allegory.Axiom.Exceptions;

public class NotFoundException(
    string? code = null,
    string? message = null)
    : AxiomException(code, message) {}