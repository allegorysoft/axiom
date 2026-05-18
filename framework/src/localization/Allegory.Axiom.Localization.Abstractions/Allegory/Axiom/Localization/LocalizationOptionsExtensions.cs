using System;
using System.Collections.Generic;
using System.Linq;

namespace Allegory.Axiom.Localization;

public static class LocalizationOptionsExtensions
{
    extension(LocalizationOptions options)
    {
        public void MapExceptionCode<T>(string exceptionCodePrefix)
        {
            var resource = ResourceNameAttribute.Get<T>();

            options.ExceptionCodeMappings[exceptionCodePrefix] = resource;
        }

        public void MapExceptionCode(string exceptionCodePrefix, string resourceName)
        {
            options.ExceptionCodeMappings[exceptionCodePrefix] = resourceName;
        }
    }

    extension(ICollection<LocalizationResourceOptions> options)
    {
        public LocalizationResourceOptions Get<T>()
        {
            return options.First(o => o.Name == ResourceNameAttribute.Get<T>());
        }

        public LocalizationResourceOptions Get(string name)
        {
            return options.First(o => o.Name == name);
        }

        public void Add<T>(string defaultCulture, params IEnumerable<string> paths)
        {
            var resource = ResourceNameAttribute.Get<T>();

            if (options.Any(o => o.Name == resource))
            {
                throw new ArgumentException($"Resource ({resource}) already exists");
            }

            options.Add(new LocalizationResourceOptions(resource, defaultCulture, paths));
        }

        public void Add(string name, string defaultCulture, params IEnumerable<string> paths)
        {
            if (options.Any(o => o.Name == name))
            {
                throw new ArgumentException($"Resource ({name}) already exists");
            }

            options.Add(new LocalizationResourceOptions(name, defaultCulture, paths));
        }
    }
}