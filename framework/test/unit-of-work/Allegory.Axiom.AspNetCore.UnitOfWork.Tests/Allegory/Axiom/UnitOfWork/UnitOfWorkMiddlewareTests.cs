using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkMiddlewareTests
{
    public UnitOfWorkMiddlewareTests()
    {
        Manager = Substitute.For<IUnitOfWorkManager>();
        UnitOfWork = Substitute.For<IUnitOfWork>();
        Options = new AspNetCoreUnitOfWorkOptions();

        Manager.Begin(Arg.Any<UnitOfWorkOptions?>()).Returns(UnitOfWork);
    }

    protected IUnitOfWorkManager Manager { get; }
    protected IUnitOfWork UnitOfWork { get; }
    protected AspNetCoreUnitOfWorkOptions Options { get; }

    private UnitOfWorkMiddleware CreateMiddleware(RequestDelegate? next = null)
    {
        return new UnitOfWorkMiddleware(
            next ?? (_ => Task.CompletedTask),
            Manager,
            Microsoft.Extensions.Options.Options.Create(Options));
    }

    private static DefaultHttpContext CreateHttpContext(HttpMethod method = HttpMethod.Get)
    {
        return new DefaultHttpContext
        {
            Request =
            {
                Method = method.ToString()
            }
        };
    }

    [Fact]
    public async Task ShouldBeginUnitOfWork()
    {
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(CreateHttpContext());

        Manager.Received(1).Begin(Arg.Any<UnitOfWorkOptions?>());
    }

    [Fact]
    public async Task ShouldNotCompleteUnitOfWork()
    {
        await CreateMiddleware().InvokeAsync(CreateHttpContext());
        await UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldDisposeUnitOfWork()
    {
        await CreateMiddleware().InvokeAsync(CreateHttpContext());

        await UnitOfWork.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ShouldDisposeUnitOfWorkEvenWhenNextThrows()
    {
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("boom"));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(CreateHttpContext()));

        await UnitOfWork.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task ShouldUseSuppressedTransactionForGetRequests()
    {
        await CreateMiddleware().InvokeAsync(CreateHttpContext());

        Manager.Received(1).Begin(Arg.Is<UnitOfWorkOptions?>(o => o != null && o.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress));
    }

    [Fact]
    public async Task ShouldUseNullOptionsForNonGetRequests()
    {
        await CreateMiddleware().InvokeAsync(CreateHttpContext(HttpMethod.Post));

        Manager.Received(1).Begin(options: null);
    }

    [Fact]
    public async Task ShouldUseCustomOptionsSelectorWhenProvided()
    {
        var custom = new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.RequiresNew);
        Options.OptionsSelector = _ => custom;

        await CreateMiddleware().InvokeAsync(CreateHttpContext());

        Manager.Received(1).Begin(custom);
    }

    [Fact]
    public async Task ShouldCallNext()
    {
        var called = false;
        await CreateMiddleware(_ =>
            {
                called = true;
                return Task.CompletedTask;
            })
            .InvokeAsync(CreateHttpContext());

        called.ShouldBeTrue();
    }
}