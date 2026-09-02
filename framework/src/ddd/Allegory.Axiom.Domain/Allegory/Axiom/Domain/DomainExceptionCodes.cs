namespace Allegory.Axiom.Domain;

public static class DomainExceptionCodes
{
    public const string Resource = "Axiom.Domain";

    public const string EntityNotFound = $"{Resource}:EntityNotFound";
    public const string EntityNotFoundByIdentifier = $"{Resource}:EntityNotFoundByIdentifier";
}