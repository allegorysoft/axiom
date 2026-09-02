using Allegory.Axiom.Exceptions;

namespace Allegory.Axiom.Domain.Entities;

public class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException(
        string? identifier = null,
        string? code = null,
        string? message = null) : base(code, message)
    {
        if (code == null)
        {
            base.Code = identifier == null
                ? DomainExceptionCodes.EntityNotFound
                : DomainExceptionCodes.EntityNotFoundByIdentifier;
        }

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            this.AddData(nameof(identifier), identifier);
        }
    }
}