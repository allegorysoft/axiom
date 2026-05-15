using System.Security.Claims;

namespace Allegory.Axiom.Security.Principal;

public interface IPrincipalAccessor
{
    ClaimsPrincipal? Current { get; }
}