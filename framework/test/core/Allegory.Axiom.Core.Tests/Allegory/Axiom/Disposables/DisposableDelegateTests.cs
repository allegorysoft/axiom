using System;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Disposables;

public class DisposableDelegateTests
{
    [Fact]
    public void ShouldValidateActionArgument()
    {
        Should.Throw<ArgumentNullException>(() => new DisposableDelegate(null!));
        Should.NotThrow(() => new DisposableDelegate(() => {}));
    }

    [Fact]
    public void ShouldInvokeActionWhenDisposed()
    {
        var invoked = false;
        using (var _ = new DisposableDelegate(() => invoked = true)) {}

        invoked.ShouldBeTrue();
    }

    [Fact]
    public void ShouldInvokeActionOnlyOnceWhenDisposedMultipleTimes()
    {
        var count = 0;
        var sut = new DisposableDelegate(() => Interlocked.Increment(ref count));

        Parallel.For(0, Environment.ProcessorCount * 4, _ => sut.Dispose());

        count.ShouldBe(1);
    }

    [Fact]
    public void ShouldPropagateExceptionWhenActionThrows()
    {
        var sut = new DisposableDelegate(() => throw new InvalidOperationException("boom"));

        Should.Throw<InvalidOperationException>(() => sut.Dispose())
            .Message.ShouldBe("boom");
    }
}

public class DisposableDelegateOfTStateTests
{
    [Fact]
    public void ShouldValidateActionArgument()
    {
        Should.Throw<ArgumentNullException>(() => new DisposableDelegate<int>(null!, 42));
        Should.NotThrow(() => new DisposableDelegate<string?>(_ => {}, null));
    }

    [Fact]
    public void ShouldInvokeActionWhenDisposed()
    {
        var state = new IntWrapper {Value = 1};
        using (var _ = new DisposableDelegate<IntWrapper>(s => s.Value = 2, state)) {}

        state.Value.ShouldBe(2);
    }

    [Fact]
    public void ShouldInvokeActionOnlyOnceWhenDisposedMultipleTimes()
    {
        var count = 0;
        var sut = new DisposableDelegate<int>(_ => Interlocked.Increment(ref count), 0);

        Parallel.For(0, Environment.ProcessorCount * 4, _ => sut.Dispose());

        count.ShouldBe(1);
    }

    [Fact]
    public void ShouldPropagateExceptionWhenActionThrows()
    {
        var sut = new DisposableDelegate<string>(_ => throw new InvalidOperationException("boom"), "x");

        Should.Throw<InvalidOperationException>(() => sut.Dispose())
            .Message.ShouldBe("boom");
    }
}

file class IntWrapper
{
    public int Value { get; set; }
}