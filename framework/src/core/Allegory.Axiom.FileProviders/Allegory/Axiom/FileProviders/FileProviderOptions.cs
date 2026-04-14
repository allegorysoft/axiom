using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace Allegory.Axiom.FileProviders;

public class FileProviderOptions
{
    public List<IFileProvider> Providers { get; } = [];
    public bool AddContentRootFileProvider { get; set; }
    public List<string> PhysicalPaths { get; } = [];
}