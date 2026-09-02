using Allegory.Axiom.Exceptions;

namespace Allegory.Axiom.Domain.Entities;

public class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException(
        string? identifier = null,
        string? code = null,
        string? message = null) : base(message: message)
    {
        // The specified entity was not found.
        // The specified entity with identifier '{identifier}' was not found.
    }
}