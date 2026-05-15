using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantProvider
{
    ValueTask<TenantContext?> TryGetAsync();
}