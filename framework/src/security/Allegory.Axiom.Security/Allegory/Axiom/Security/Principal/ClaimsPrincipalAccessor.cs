using System.Security.Claims;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Security.Principal;

[Dependency(Strategy = RegistrationStrategy.TryAdd)]
public class ClaimsPrincipalAccessor : IPrincipalAccessor, ISingletonService
{
    public virtual ClaimsPrincipal? Current => ClaimsPrincipal.Current;
}