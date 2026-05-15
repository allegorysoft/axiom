namespace Allegory.Axiom.MultiTenancy;

public interface ITenantNormalizer
{
    string NormalizeName(string name);
}