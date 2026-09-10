namespace Allegory.Axiom.MultiTenancy;

public static class MultiTenancyExceptionCodes
{
    public const string Resource = "Axiom.MultiTenancy";

    public const string TenantNotFound = $"{Resource}:TenantNotFound";
    public const string TenantNotActive = $"{Resource}:TenantNotActive";
    public const string PrincipalHasNoAccess = $"{Resource}:PrincipalHasNoAccess";
}