namespace Allegory.Axiom.Domain;

public static class DomainExceptionCodes
{
    public const string Resource = "Axiom.Domain";

    public static string EntityNotFound { get; } = $"{Resource}:EntityNotFound";
}