using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Allegory.Axiom.FileProviders;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        public void AddFileProvider(IFileProvider fileProvider)
        {
            collection.Configure<FileProviderOptions>(options =>
            {
                options.Providers.Add(fileProvider);
            });
        }

        public void AddEmbeddedFileProvider<T>(string? baseNamespace = null)
        {
            collection.Configure<FileProviderOptions>(options =>
            {
                options.Providers.Add(new EmbeddedFileProvider(typeof(T).Assembly, baseNamespace));
            });
        }

        public void AddEmbeddedFileProvider(Assembly assembly, string? baseNamespace = null)
        {
            collection.Configure<FileProviderOptions>(options =>
            {
                options.Providers.Add(new EmbeddedFileProvider(assembly, baseNamespace));
            });
        }

        public void AddPhysicalFileProvider(
            string path,
            ExclusionFilters? exclusionFilters = null)
        {
            collection.Configure<FileProviderOptions>(options =>
            {
                options.Providers.Add(
                    exclusionFilters.HasValue
                        ? new PhysicalFileProvider(path, exclusionFilters.Value)
                        : new PhysicalFileProvider(path));
            });
        }
    }
}