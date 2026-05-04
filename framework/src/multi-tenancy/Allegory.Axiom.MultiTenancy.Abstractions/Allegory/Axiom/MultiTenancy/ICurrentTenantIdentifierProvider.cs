using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantIdentifierProvider : ISingletonService
{
    ValueTask<string?> TryGetAsync();
}