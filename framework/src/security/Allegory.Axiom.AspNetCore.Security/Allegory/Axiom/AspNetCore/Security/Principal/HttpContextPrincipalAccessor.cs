using System.Security.Claims;
using Allegory.Axiom.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace Allegory.Axiom.AspNetCore.Security.Principal;

public class HttpContextPrincipalAccessor(IHttpContextAccessor httpContextAccessor) : ClaimsPrincipalAccessor
{
    public override ClaimsPrincipal? Current => HttpContextAccessor.HttpContext?.User ?? base.Current;

    protected virtual IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;
}