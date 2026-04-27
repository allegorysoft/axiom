using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Disposables;

public class AsyncDisposableDelegateTests
{
    [Fact]
    public void ShouldValidateActionArgument()
    {
        Should.Throw<ArgumentNullException>(() => new AsyncDisposableDelegate(null!));
        Should.NotThrow(() => new AsyncDisposableDelegate(() => ValueTask.CompletedTask));
    }

    [Fact]
    public async Task ShouldInvokeActionWhenDisposed()
    {
        var invoked = false;
        await using (var _ = new AsyncDisposableDelegate(async () =>
        {
            await Task.Yield();
            invoked = true;
        })) {}

        invoked.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldInvokeActionOnlyOnceWhenDisposedMultipleTimes()
    {
        var count = 0;
        var sut = new AsyncDisposableDelegate(() =>
        {
            Interlocked.Increment(ref count);
            return ValueTask.CompletedTask;
        });

        await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount * 4)
            .Select(_ => sut.DisposeAsync().AsTask()));

        count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldPropagateExceptionWhenActionThrows()
    {
        var sut = new AsyncDisposableDelegate(() => throw new InvalidOperationException("boom"));

        (await Should.ThrowAsync<InvalidOperationException>(async () => await sut.DisposeAsync()))
            .Message.ShouldBe("boom");
    }
}

public class AsyncDisposableDelegateOfTStateTests
{
    [Fact]
    public void ShouldValidateActionArgument()
    {
        Should.Throw<ArgumentNullException>(() => new AsyncDisposableDelegate<int>(null!, 42));
        Should.NotThrow(() => new AsyncDisposableDelegate<string?>(_ => ValueTask.CompletedTask, null));
    }

    [Fact]
    public async Task ShouldInvokeActionWhenDisposed()
    {
        var state = new IntWrapper {Value = 1};
        await using (var _ = new AsyncDisposableDelegate<IntWrapper>(async s =>
        {
            await Task.Yield();
            s.Value = 2;
        }, state)) {}

        state.Value.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldInvokeActionOnlyOnceWhenDisposedMultipleTimes()
    {
        var count = 0;
        var sut = new AsyncDisposableDelegate<int>(_ =>
        {
            Interlocked.Increment(ref count);
            return ValueTask.CompletedTask;
        }, 0);

        await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount * 4)
            .Select(_ => sut.DisposeAsync().AsTask()));

        count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldPropagateExceptionWhenActionThrows()
    {
        var sut = new AsyncDisposableDelegate<string>(_ => throw new InvalidOperationException("boom"), "x");

        (await Should.ThrowAsync<InvalidOperationException>(async () => await sut.DisposeAsync()))
            .Message.ShouldBe("boom");
    }
}

file class IntWrapper
{
    public int Value { get; set; }
}