using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Allegory.Axiom.Localization;

public class LocalizationResourceOptions
{
    private readonly List<string> _paths;

    internal LocalizationResourceOptions(
        string name,
        string defaultCulture,
        params IEnumerable<string> paths)
    {
        Name = name;
        DefaultCulture = new CultureInfo(defaultCulture);
        _paths = paths.ToList();
    }

    public string Name { get; }
    public CultureInfo DefaultCulture { get; }
    public IReadOnlyList<string> Paths => _paths;

    public void AddPaths(params IEnumerable<string> paths) => _paths.AddRange(paths);

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is LocalizationResourceOptions other && Name == other.Name;
    }
}