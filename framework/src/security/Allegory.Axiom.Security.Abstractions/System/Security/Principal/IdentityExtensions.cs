using System.Security.Claims;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.Security;

namespace System.Security.Principal;

public static class IdentityExtensions
{
    extension(IIdentity identity)
    {
        public string GetNameIdentifier()
        {
            return identity.FindNameIdentifier() ??
                   throw new AuthorizationException(SecurityExceptionCodes.NameIdentifierNotFound);
        }

        public string? FindNameIdentifier()
        {
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}