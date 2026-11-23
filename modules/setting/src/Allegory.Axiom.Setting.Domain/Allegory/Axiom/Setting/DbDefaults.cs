using Allegory.Axiom.Data;

namespace Allegory.Axiom.Setting;

public static class DbDefaults
{
    public static string TablePrefix { get; set; } = AxiomDbDefaults.TablePrefix;
    public static string? Schema { get; set; } = AxiomDbDefaults.Schema;
}