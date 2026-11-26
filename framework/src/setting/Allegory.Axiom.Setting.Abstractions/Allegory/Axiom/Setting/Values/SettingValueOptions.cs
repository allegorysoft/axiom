using System;
using System.Collections.Generic;
using System.Linq;

namespace Allegory.Axiom.Setting;

public class SettingValueOptions
{
    private List<Type> _contributors = new();
    public IReadOnlyList<Type> Contributors => _contributors;

    public void AddContributor<T>(int? index = null) where T : ISettingValueContributor
    {
        if (_contributors.Contains(typeof(T)))
        {
            return;
        }

        if (index.HasValue)
        {
            _contributors.Insert(index.Value, typeof(T));
        }
        else
        {
            _contributors.Add(typeof(T));
        }
    }

    public void SetContributor(params IEnumerable<Type> contributors)
    {
        var list = contributors.ToList();

        if (!list.Any(c => typeof(ISettingValueContributor).IsAssignableFrom(c)))
        {
            throw new InvalidOperationException(
                $"All contributors must implement {nameof(ISettingValueContributor)}.");
        }

        _contributors.Clear();
        _contributors = list.ToList();
    }
}