using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Allegory.Axiom.Localization;

public class LocalizationResourceOptions
{
    internal LocalizationResourceOptions(
        string resource,
        string defaultCulture,
        params IEnumerable<string> paths)
    {
        Resource = resource;
        DefaultCulture = new CultureInfo(defaultCulture);
        Paths = paths.ToArray();
    }

    public string Resource { get; }
    public CultureInfo DefaultCulture { get; }
    public string[] Paths { get; }

    public override int GetHashCode()
    {
        return Resource.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is LocalizationResourceOptions other && Resource == other.Resource;
    }
}