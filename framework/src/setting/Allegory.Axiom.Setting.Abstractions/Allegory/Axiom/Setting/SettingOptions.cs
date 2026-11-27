using System.Collections.Generic;
using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.Setting;

public class SettingOptions : IExtraProperties
{
    public SettingValueOptions SettingValue { get; set; } = new();
    public IDictionary<string, object> ExtraProperties { get; } = new Dictionary<string, object>();
}