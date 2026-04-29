using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantProvider : ISingletonService
{
    ValueTask<TenantContext?> TryGetAsync();
}