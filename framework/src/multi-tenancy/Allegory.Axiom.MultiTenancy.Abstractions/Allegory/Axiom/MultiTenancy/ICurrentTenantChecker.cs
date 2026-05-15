using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public interface ICurrentTenantChecker
{
    Task CheckAsync(TenantContext tenant);
}