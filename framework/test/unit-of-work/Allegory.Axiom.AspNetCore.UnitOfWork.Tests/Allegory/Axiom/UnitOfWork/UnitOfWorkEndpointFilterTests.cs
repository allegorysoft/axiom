using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

[SuppressMessage("Usage", "xUnit1051:Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken")]
public class UnitOfWorkEndpointFilterTests
{
    public UnitOfWorkEndpointFilterTests()
    {
        UnitOfWork = Substitute.For<IUnitOfWork>();
        Manager = Substitute.For<IUnitOfWorkManager>();
        Manager.Begin(Arg.Any<UnitOfWorkOptions>()).Returns(UnitOfWork);
        Options = Microsoft.Extensions.Options.Options.Create(new AspNetCoreUnitOfWorkOptions());

        Filter = new UnitOfWorkEndpointFilter(Manager, Options);
    }

    protected IUnitOfWorkManager Manager { get; }
    protected IUnitOfWork UnitOfWork { get; }
    protected IOptions<AspNetCoreUnitOfWorkOptions> Options { get; }
    protected UnitOfWorkEndpointFilter Filter { get; }

    private EndpointFilterInvocationContext CreateContext() =>
        new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

    [Fact]
    public async Task ShouldCallNext()
    {
        var called = false;
        await Filter.InvokeAsync(CreateContext(), _ =>
        {
            called = true;
            return ValueTask.FromResult<object?>(null);
        });

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldBeginUnitOfWork()
    {
        await Filter.InvokeAsync(
            CreateContext(),
            _ => ValueTask.FromResult<object?>(null));

        Manager.Received(1).Begin(Arg.Any<UnitOfWorkOptions?>());
    }

    [Fact]
    public async Task ShouldCompleteAndReturnResultOnSuccess()
    {
        var expected = new object();
        var ctx = CreateContext();

        var result = await Filter.InvokeAsync(
            ctx,
            _ => ValueTask.FromResult<object?>(expected));

        result.ShouldBe(expected);
        await UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await UnitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldRollbackAndRethrowOnException()
    {
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
        var ctx = CreateContext();

        await Filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        await UnitOfWork.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ShouldDisposeUnitOfWorkEvenWhenNextThrows()
    {
        var ctx = CreateContext();

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await Filter.InvokeAsync(ctx, _ => throw new InvalidOperationException()));

        await UnitOfWork.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ShouldUseSuppressedTransactionForGetOrQueryRequests()
    {
        var getRequest = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext()
        {
            Request = {Method = HttpMethods.Get}
        });
        await Filter.InvokeAsync(getRequest, _ => ValueTask.FromResult<object?>(null));

        var queryRequest = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext()
        {
            Request = {Method = HttpMethods.Get}
        });
        await Filter.InvokeAsync(queryRequest, _ => ValueTask.FromResult<object?>(null));

        Manager.Received(2).Begin(Arg.Is<UnitOfWorkOptions?>(o =>
            o != null && o.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress));
    }

    [Fact]
    public async Task ShouldUseNullOptionsForNonGetOrQueryRequests()
    {
        await Filter.InvokeAsync(CreateContext(), _ => ValueTask.FromResult<object?>(null));

        Manager.Received(1).Begin(options: null);
    }

    [Fact]
    public async Task ShouldUseCustomOptionsSelectorWhenProvided()
    {
        var custom = new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.RequiresNew);
        Options.Value.OptionsSelector = _ => custom;

        await Filter.InvokeAsync(CreateContext(), _ => ValueTask.FromResult<object?>(null));

        Manager.Received(1).Begin(custom);
    }
    
    [Fact]
    public async Task ShouldPassHttpContextRequestServicesToBegin()
    {
        var httpContext = new DefaultHttpContext();
        var requestServices = Substitute.For<IServiceProvider>();
        httpContext.RequestServices = requestServices;

        await Filter.InvokeAsync(
            new DefaultEndpointFilterInvocationContext(httpContext, []),
            _ => ValueTask.FromResult<object?>(null));

        Manager.Received(1).Begin(Arg.Any<UnitOfWorkOptions?>(), requestServices);
    }
    
    [Fact]
    public async Task ShouldPassHttpContextRequestAbortedTokenToBegin()
    {
        var httpContext = new DefaultHttpContext();
        var cts = new CancellationTokenSource();
        httpContext.RequestAborted = cts.Token;

        await Filter.InvokeAsync(
            new DefaultEndpointFilterInvocationContext(httpContext, []),
            _ => ValueTask.FromResult<object?>(null));

        Manager.Received(1).Begin(Arg.Any<UnitOfWorkOptions?>(), Arg.Any<IServiceProvider?>(), httpContext.RequestAborted);
    }
}