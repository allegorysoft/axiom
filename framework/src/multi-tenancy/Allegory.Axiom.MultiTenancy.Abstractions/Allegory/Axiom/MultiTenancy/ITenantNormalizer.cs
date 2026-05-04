using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ITenantNormalizer : ISingletonService
{
    string NormalizeName(string name);
}