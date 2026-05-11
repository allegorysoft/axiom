using System;

namespace Allegory.Axiom.Exceptions;

public class AxiomException(string? code = null, string? message = null) : Exception(message)
{
    public string? Code { get; init; } = code;
}