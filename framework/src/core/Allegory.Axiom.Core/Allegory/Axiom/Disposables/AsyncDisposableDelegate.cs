using System;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.Disposables;

public sealed class AsyncDisposableDelegate(Func<ValueTask> action) : IAsyncDisposable
{
    private Func<ValueTask>? _delegate = action ?? throw new ArgumentNullException(nameof(action));

    public async ValueTask DisposeAsync()
    {
        var action = Interlocked.Exchange(ref _delegate, null);
        if (action is not null)
        {
            await action();
        }
    }
}

public sealed class AsyncDisposableDelegate<TState>(Func<TState, ValueTask> action, TState state) : IAsyncDisposable
{
    private Func<TState, ValueTask>? _delegate = action ?? throw new ArgumentNullException(nameof(action));

    public async ValueTask DisposeAsync()
    {
        var action = Interlocked.Exchange(ref _delegate, null);
        if (action is not null)
        {
            await action(state);
        }
    }
}