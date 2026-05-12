namespace Allegory.Axiom.Exceptions;

public class BusinessException(
    string? code = null,
    string? message = null)
    : AxiomException(code, message) {}