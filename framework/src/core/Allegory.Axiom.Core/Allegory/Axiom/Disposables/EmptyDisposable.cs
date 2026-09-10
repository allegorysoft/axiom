using System;
using System.Threading.Tasks;

namespace Allegory.Axiom.Disposables;

public sealed class EmptyDisposable : IDisposable, IAsyncDisposable
{
    public static EmptyDisposable Instance { get; } = new();

    private EmptyDisposable() { }

    public void Dispose() { }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}