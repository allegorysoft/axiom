using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;

namespace Allegory.Axiom;

internal sealed class TestContainer(IContainer container) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => container.DisposeAsync();
}