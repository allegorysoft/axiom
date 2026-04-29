using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantResolver : ISingletonService
{
    ValueTask<string?> TryGetAsync();
}