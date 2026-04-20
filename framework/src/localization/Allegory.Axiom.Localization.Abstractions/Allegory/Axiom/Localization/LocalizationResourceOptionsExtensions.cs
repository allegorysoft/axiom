using System;
using System.Collections.Generic;
using System.Linq;

namespace Allegory.Axiom.Localization;

public static class LocalizationResourceOptionsExtensions
{
    extension(ICollection<LocalizationResourceOptions> options)
    {
        public void Add<T>(string defaultCulture, params IEnumerable<string> paths)
        {
            var resource = typeof(T).FullName;
            ArgumentException.ThrowIfNullOrEmpty(resource);

            if (options.Any(o => o.Resource == resource))
            {
                throw new ArgumentException($"Resource ({resource}) already exists");
            }

            options.Add(new LocalizationResourceOptions(resource, defaultCulture, paths));
        }

        public void Add(string resource, string defaultCulture, params IEnumerable<string> paths)
        {
            if (options.Any(o => o.Resource == resource))
            {
                throw new ArgumentException($"Resource ({resource}) already exists");
            }

            options.Add(new LocalizationResourceOptions(resource, defaultCulture, paths));
        }
    }
}