namespace Allegory.Axiom.MultiTenancy;

public static class MultiTenancyExceptionCodes
{
    public const string Resource = "Axiom.MultiTenancy";

    public static string TenantNotFound { get; } = $"{Resource}:TenantNotFound";
    public static string TenantNotActive { get; } = $"{Resource}:TenantNotActive";
    public static string PrincipalHasNoAccess { get; } = $"{Resource}:PrincipalHasNoAccess";
}