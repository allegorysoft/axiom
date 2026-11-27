using Allegory.Axiom.Data;

namespace Allegory.Axiom.Setting;

public static class SettingDefaults
{
    public static bool IsSynchronizationEnabled { get; set; } = true;
    public static string TablePrefix { get; set; } = AxiomDbDefaults.TablePrefix;
    public static string? Schema { get; set; } = AxiomDbDefaults.Schema;
}