using System.Security.Claims;

namespace System.Security.Principal;

public static class ClaimsIdentityExtensions
{
    extension(IIdentity identity)
    {
        public string? FindId()
        {
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}