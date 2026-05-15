using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public class TenantNormalizer : ITenantNormalizer, ISingletonService
{
    public string NormalizeName(string name)
    {
        return name.ToUpperInvariant();
    }
}