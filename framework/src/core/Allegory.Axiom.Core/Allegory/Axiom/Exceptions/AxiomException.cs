using System;

namespace Allegory.Axiom.Exceptions;

public abstract class AxiomException(
    string? code = null,
    string? message = null)
    : Exception
{
    public virtual string? Code { get; init; } = code;
    public override string Message { get; } = message ?? string.Empty;
}