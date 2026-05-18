namespace Allegory.Axiom.MultiTenancy;

public class AspNetCoreMultiTenancyOptions
{
    public string HeaderKey { get; set; } = "Tenant";
    public string QueryKey { get; set; } = "__tenant";
    public string RouteKey { get; set; } = "tenant";
}