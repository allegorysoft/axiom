using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Allegory.Axiom.Localization;

public class LocalizationResourceOptions
{
    internal LocalizationResourceOptions(
        string name,
        string defaultCulture,
        params IEnumerable<string> paths)
    {
        Name = name;
        DefaultCulture = new CultureInfo(defaultCulture);
        Paths = paths.ToArray();
    }

    public string Name { get; }
    public CultureInfo DefaultCulture { get; }
    public string[] Paths { get; }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is LocalizationResourceOptions other && Name == other.Name;
    }
}