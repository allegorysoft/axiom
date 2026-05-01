using System.Security.Claims;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Security.Principal;

public interface IPrincipalAccessor : ISingletonService
{
    ClaimsPrincipal? Current { get; }
}