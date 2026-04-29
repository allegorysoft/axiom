namespace Allegory.Axiom.MultiTenancy;

public class TenantNormalizer : ITenantNormalizer
{
    public string NormalizeName(string name)
    {
        return name.ToUpperInvariant();
    }
}