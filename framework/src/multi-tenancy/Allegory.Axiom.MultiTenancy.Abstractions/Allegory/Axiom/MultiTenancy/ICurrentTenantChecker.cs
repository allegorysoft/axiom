using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantChecker : ISingletonService
{
    Task CheckAsync(TenantContext tenant);
}