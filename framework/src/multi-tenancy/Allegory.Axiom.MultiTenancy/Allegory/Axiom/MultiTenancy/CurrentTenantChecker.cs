using System.Threading.Tasks;

namespace Allegory.Axiom.MultiTenancy;

public class CurrentTenantChecker : ICurrentTenantChecker
{
    public Task CheckAsync(TenantContext tenant)
    {
        //TODO: Implement

        return Task.CompletedTask;
    }
}