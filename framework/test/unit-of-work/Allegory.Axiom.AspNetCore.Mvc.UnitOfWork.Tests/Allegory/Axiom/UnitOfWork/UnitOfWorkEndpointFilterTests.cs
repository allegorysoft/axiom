using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkEndpointFilterTests
{
    public UnitOfWorkEndpointFilterTests()
    {
        Manager = Substitute.For<IUnitOfWorkManager>();
        UnitOfWork = Substitute.For<IUnitOfWork>();
        Filter = new UnitOfWorkEndpointFilter(Manager);
    }

    protected IUnitOfWorkManager Manager { get; }
    protected IUnitOfWork UnitOfWork { get; }
    protected UnitOfWorkEndpointFilter Filter { get; }

    private EndpointFilterInvocationContext CreateContext() =>
        new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

    [Fact]
    public async Task ShouldSkipUnitOfWorkWhenCurrentUnitOfWorkNotExists()
    {
        Manager.Current.Returns((IUnitOfWork?) null);
        var nextCalled = false;
        var ctx = CreateContext();

        await Filter.InvokeAsync(ctx,
            _ =>
            {
                nextCalled = true;
                return ValueTask.FromResult<object?>(null);
            });

        nextCalled.ShouldBeTrue();
        await UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldCompleteAndReturnResultOnSuccess()
    {
        Manager.Current.Returns(UnitOfWork);
        var expected = new object();
        var ctx = CreateContext();

        var result = await Filter.InvokeAsync(ctx,
            _ => ValueTask.FromResult<object?>(expected));

        result.ShouldBe(expected);
        await UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldRollbackAndRethrowOnException()
    {
        Manager.Current.Returns(UnitOfWork);
        var ex = new InvalidOperationException("boom");
        var ctx = CreateContext();

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            Filter.InvokeAsync(ctx, _ => throw ex).AsTask());

        await UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        ex.ShouldBe(exception);
    }

    [Fact]
    public async Task ShouldDisposeUnitOfWork()
    {
        Manager.Current.Returns(UnitOfWork);
        var ctx = CreateContext();

        await Filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        await UnitOfWork.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ShouldUseRequestAbortedTokenForComplete()
    {
        Manager.Current.Returns(UnitOfWork);
        var httpContext = new DefaultHttpContext();
        var cts = new CancellationTokenSource();
        httpContext.RequestAborted = cts.Token;

        await Filter.InvokeAsync(
            new DefaultEndpointFilterInvocationContext(httpContext, []),
            _ => ValueTask.FromResult<object?>(null));

        await UnitOfWork.Received(1).TryCompleteAsync(cts.Token);
    }
}