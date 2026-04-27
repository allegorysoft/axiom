using System;
using System.Threading;

namespace Allegory.Axiom.Disposables;

public sealed class DisposableDelegate(Action action) : IDisposable
{
    private Action? _delegate = action ?? throw new ArgumentNullException(nameof(action));

    public void Dispose()
    {
        Interlocked.Exchange(ref _delegate, null)?.Invoke();
    }
}

public sealed class DisposableDelegate<TState>(Action<TState> action, TState state) : IDisposable
{
    private Action<TState>? _delegate = action ?? throw new ArgumentNullException(nameof(action));

    public void Dispose()
    {
        Interlocked.Exchange(ref _delegate, null)?.Invoke(state);
    }
}