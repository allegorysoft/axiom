using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkActionFilterTests
{
    public UnitOfWorkActionFilterTests()
    {
        Manager = Substitute.For<IUnitOfWorkManager>();
        UnitOfWork = Substitute.For<IUnitOfWork>();
        Filter = new UnitOfWorkActionFilter(Manager);
    }

    protected IUnitOfWorkManager Manager { get; }
    protected IUnitOfWork UnitOfWork { get; }
    protected UnitOfWorkActionFilter Filter { get; }

    private ActionExecutingContext CreateExecutingContext() =>
        new(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [],
            new Dictionary<string, object?>(),
            new object());

    private static ActionExecutedContext CreateExecutedContext(
        ActionExecutingContext ctx,
        Exception? exception = null,
        bool exceptionHandled = false) =>
        new(ctx, [], new())
        {
            Exception = exception,
            ExceptionHandled = exceptionHandled,
        };

    [Fact]
    public async Task ShouldSkipUnitOfWorkWhenCurrentUnitOfWorkNotExists()
    {
        Manager.Current.Returns((IUnitOfWork?) null);
        UnitOfWork.TryCompleteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var nextCalled = false;
        var ctx = CreateExecutingContext();

        await Filter.OnActionExecutionAsync(
            ctx,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(CreateExecutedContext(ctx));
            });

        nextCalled.ShouldBeTrue();
        await UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldCompleteWhenNoException()
    {
        Manager.Current.Returns(UnitOfWork);
        var ctx = CreateExecutingContext();

        await Filter.OnActionExecutionAsync(
            ctx,
            () => Task.FromResult(CreateExecutedContext(ctx)));

        await UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldCompleteWhenExceptionIsHandled()
    {
        Manager.Current.Returns(UnitOfWork);
        var ctx = CreateExecutingContext();

        await Filter.OnActionExecutionAsync(
            ctx,
            () => Task.FromResult(CreateExecutedContext(ctx,
                exception: new InvalidOperationException(),
                exceptionHandled: true)));

        await UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldRollbackWhenUnhandledException()
    {
        Manager.Current.Returns(UnitOfWork);
        var ex = new InvalidOperationException("boom");
        var ctx = CreateExecutingContext();

        await Filter.OnActionExecutionAsync(
            ctx,
            () => Task.FromResult(CreateExecutedContext(ctx, exception: ex)));

        await UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldDisposeUnitOfWork()
    {
        Manager.Current.Returns(UnitOfWork);
        var ctx = CreateExecutingContext();

        await Filter.OnActionExecutionAsync(ctx,
            () => Task.FromResult(CreateExecutedContext(ctx)));

        await UnitOfWork.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ShouldDisposeEvenWhenCompleteThrows()
    {
        Manager.Current.Returns(UnitOfWork);
        UnitOfWork.TryCompleteAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException());
        var ctx = CreateExecutingContext();

        await Should.ThrowAsync<InvalidOperationException>(() =>
            Filter.OnActionExecutionAsync(ctx,
                () => Task.FromResult(CreateExecutedContext(ctx))));

        await UnitOfWork.Received(1).DisposeAsync();
    }
}