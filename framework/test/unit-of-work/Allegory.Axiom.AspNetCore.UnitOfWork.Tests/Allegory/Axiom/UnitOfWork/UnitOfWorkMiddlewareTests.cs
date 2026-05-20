using System.Collections.Generic;
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
    public async Task ShouldCompleteUnitOfWorkAfterNext()
    {
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(CreateHttpContext());

        await UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldAlwaysCallNext()
    {
        var isNextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            isNextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(CreateHttpContext());

        isNextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCompleteAfterCallingNext()
    {
        var callOrder = new List<string>();
        UnitOfWork.When(x => x.CompleteAsync())
            .Do(_ => callOrder.Add("complete"));
        var middleware = CreateMiddleware(_ =>
        {
            callOrder.Add("next");
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(CreateHttpContext());

        callOrder.ShouldBe(["next", "complete"]);
    }

    [Fact]
    public async Task ShouldUseSuppressedTransactionForGetRequests()
    {
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(CreateHttpContext());

        Manager.Received(1).Begin(Arg.Is<UnitOfWorkOptions?>(o => o != null && o.TransactionBehavior == UnitOfWorkTransactionBehavior.Suppress));
    }

    [Fact]
    public async Task ShouldUseNullOptionsForNonGetRequests()
    {
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(CreateHttpContext(HttpMethod.Post));

        Manager.Received(1).Begin(options: null);
    }

    [Fact]
    public async Task ShouldUseCustomOptionsSelectorWhenProvided()
    {
        var customOptions = new UnitOfWorkOptions(UnitOfWorkTransactionBehavior.Required);
        Options.OptionsSelector = _ => customOptions;
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(CreateHttpContext());

        Manager.Received(1).Begin(customOptions);
    }

    [Fact]
    public async Task ShouldDisposeUnitOfWorkAfterCompletion()
    {
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(CreateHttpContext());

        await UnitOfWork.Received(1).DisposeAsync();
    }
}