using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Allegory.Axiom.Security.Principal;

public class HttpContextPrincipalAccessor(IHttpContextAccessor httpContextAccessor) : ClaimsPrincipalAccessor
{
    public override ClaimsPrincipal? Current => HttpContextAccessor.HttpContext?.User ?? base.Current;

    protected virtual IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;
}