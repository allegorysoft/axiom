using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantIdentifierProvider
{
    ValueTask<string?> TryGetAsync();
}