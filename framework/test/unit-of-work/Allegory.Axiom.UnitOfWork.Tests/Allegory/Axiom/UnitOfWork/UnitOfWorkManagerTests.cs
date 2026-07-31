using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

[SuppressMessage("Usage",
    "xUnit1051:Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken")]
public class UnitOfWorkManagerTests(UnitOfWorkManagerFixture fixture) : IClassFixture<UnitOfWorkManagerFixture>
{
    protected IUnitOfWorkManager Manager { get; } = fixture.Service<IUnitOfWorkManager>();

    [Fact]
    public void ShouldCreateUnitOfWork()
    {
        using (var root = Manager.Begin())
        {
            Manager.Current.ShouldNotBeNull();
            Manager.Current.ShouldBe(root);
        }

        Manager.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldCreateChildUnitOfWorkWhenParentExists()
    {
        using (var root = Manager.Begin())
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = Manager.Begin())
            {
                Manager.Current.ShouldBe(child);
                Manager.Current.ShouldBeOfType<ChildUnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldRestoreParentUnitOfWorkAfterChildUnitOfWorkDisposed()
    {
        using (var root = Manager.Begin())
        {
            Manager.Current.ShouldBe(root);

            using (var child = Manager.Begin())
            {
                Manager.Current.ShouldBe(child);
            }

            Manager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public async Task ShouldRestoreParentUnitOfWorkAfterSubUnitOfWorkAsyncDisposed()
    {
        await using (var root = Manager.Begin())
        {
            Manager.Current.ShouldBe(root);

            await using (var child = Manager.Begin(new UnitOfWorkOptions(
                             transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
            {
                Manager.Current.ShouldBe(child);
            }

            Manager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public async Task ShouldNotRestoreParentUnitOfWorkWithoutAwaitingUntilSecondTaskCompletes()
    {
        // We use `AsyncLocalContext<>` to mutate the parent task's current state (execution
        // context value) from a child execution context. `uow.DisposeAsync` runs in its own
        // execution context but still needs to mutate the caller's context, since any task
        // running at the sametime shares and mutates that same context, a unit of work
        // shouldn't let another concurrently running method mutate it out from under us.

        await using (var root = Manager.Begin())
        {
            var rootSignal = new TaskCompletionSource();
            var childSignal = new TaskCompletionSource();
            var task = Job(rootSignal, childSignal);
            await rootSignal.Task; // Wait for task (Job) changes the `Manager.Current` to child uow

            Manager.Current.ShouldNotBe(root);
            childSignal.SetResult();
            await task; // When task is over child.DisposeAsync restore context 
            Manager.Current.ShouldBe(root);
        }

        return;

        async Task Job(TaskCompletionSource rootSignal, TaskCompletionSource childSignal)
        {
            await using var child = Manager.Begin();
            rootSignal.SetResult();
            Manager.Current.ShouldBe(child);
            await childSignal.Task;
        }
    }

    [Fact]
    public void ShouldUseParentPropertiesWhenUnitOfWorkIsChild()
    {
        using (var root = Manager.Begin())
        {
            root.Items["key"] = "value";
            using (var child = Manager.Begin())
            {
                Manager.RequiredCurrent.Items["key"].ShouldBe("value");
                root.Items.ShouldBe(child.Items);
            }
        }
    }

    [Fact]
    public void ShouldCreateSubRootUnitOfWorkWhenTransactionBehaviorIsRequiresNew()
    {
        using (var root = Manager.Begin())
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = Manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
            {
                Manager.Current.ShouldBe(subRoot);
                Manager.Current.ShouldBeOfType<UnitOfWork>();
            }

            Manager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public void ShouldCreateChildUnitOfWorkWhenTransactionBehaviorCompatible()
    {
        // Required, Required
        using (var root = Manager.Begin())
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = Manager.Begin())
            {
                Manager.Current.ShouldBe(child);
                Manager.Current.ShouldBeOfType<ChildUnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }

        // RequiresNew, Required
        using (var root = Manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = Manager.Begin())
            {
                Manager.Current.ShouldBe(child);
                Manager.Current.ShouldBeOfType<ChildUnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }

        // Suppress, Suppress
        using (var root = Manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = Manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                Manager.Current.ShouldBe(child);
                Manager.Current.ShouldBeOfType<ChildUnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldCreateSubRootUnitOfWorkWhenTransactionBehaviorIncompatible()
    {
        // Required, Suppress
        using (var root = Manager.Begin())
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = Manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                Manager.Current.ShouldBe(subRoot);
                Manager.Current.ShouldBeOfType<UnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }

        // Suppress, Required
        using (var root = Manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = Manager.Begin())
            {
                Manager.Current.ShouldBe(subRoot);
                Manager.Current.ShouldBeOfType<UnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }

        // RequiresNew, Suppress
        using (var root = Manager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = Manager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                Manager.Current.ShouldBe(subRoot);
                Manager.Current.ShouldBeOfType<UnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }
    }

    // Options

    [Fact]
    public void ShouldApplyDefaultOptionsWhenPreferredOptionsNull()
    {
        var options = fixture.Service<IOptions<UnitOfWorkOptions>>().Value;

        using var uow = Manager.Begin();

        Manager.RequiredCurrent.Options.ShouldBe(options);
        Manager.RequiredCurrent.Options.Timeout.ShouldBe(options.Timeout);
    }

    [Fact]
    public void ShouldApplyPreferredOptionsWhenPreferredOptionsNotNull()
    {
        var preferred = new UnitOfWorkOptions(timeout: TimeSpan.FromMinutes(1));
        using var uow = Manager.Begin(preferred);

        Manager.RequiredCurrent.Options.ShouldBe(preferred);
        Manager.RequiredCurrent.Options.Timeout.ShouldBe(preferred.Timeout);
    }

    [Fact]
    public void ShouldFallbackDefaultOptionsWhenPreferredOptionsPropertyIsNull()
    {
        var options = fixture.Service<IOptions<UnitOfWorkOptions>>().Value;

        var preferred = new UnitOfWorkOptions(isolationLevel: IsolationLevel.ReadUncommitted);
        using var uow = Manager.Begin(preferred);

        Manager.RequiredCurrent.Options.ShouldBe(preferred);
        Manager.RequiredCurrent.Options.IsolationLevel.ShouldBe(preferred.IsolationLevel);
        Manager.RequiredCurrent.Options.Timeout.ShouldBe(options.Timeout);
    }

    // ServiceProvider

    [Fact]
    public void ShouldCreateNewServiceProviderWhenNoneProvidedAndNoParent()
    {
        using var uow = Manager.Begin();

        uow.ServiceProvider.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldUseProvidedServiceProviderWhenBeginCalledWithServiceProvider()
    {
        var customProvider = fixture.Service<IServiceProvider>();

        using var uow = Manager.Begin(serviceProvider: customProvider);

        uow.ServiceProvider.ShouldBe(customProvider);
    }

    [Fact]
    public void ShouldUseParentServiceProviderWhenChildBegunWithoutExplicitProvider()
    {
        using var root = Manager.Begin();

        using var child = Manager.Begin();

        child.ServiceProvider.ShouldBeSameAs(root.ServiceProvider);
    }

    [Fact]
    public void ShouldUseExplicitServiceProviderForChildWhenProvided()
    {
        var customProvider = fixture.Service<IServiceProvider>();

        using var root = Manager.Begin();
        using var child = Manager.Begin(serviceProvider: customProvider);

        child.ServiceProvider.ShouldBeSameAs(customProvider);
        child.ServiceProvider.ShouldNotBe(root.ServiceProvider);
    }

    [Fact]
    public void ShouldUseParentServiceProviderWhenSubRootBegunWithoutExplicitProvider()
    {
        // RequiresNew guarantees an independent transaction boundary, not an independent
        // DI scope. Without an explicit provider, the sub-root inherits the ambient
        // ServiceProvider. Callers needing scope isolation must create their own
        // IServiceScope and pass its provider explicitly to Begin.

        using var root = Manager.Begin();

        using var subRoot = Manager.Begin(new UnitOfWorkOptions(
            transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew));

        subRoot.ServiceProvider.ShouldBeSameAs(root.ServiceProvider);
    }

    [Fact]
    public void ShouldUseExplicitServiceProviderForSubRootWhenProvided()
    {
        var customProvider = fixture.Service<IServiceProvider>();
        using var root = Manager.Begin();

        using var subRoot = Manager.Begin(
            new UnitOfWorkOptions(transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew),
            serviceProvider: customProvider);

        subRoot.ServiceProvider.ShouldBeSameAs(customProvider);
        subRoot.ServiceProvider.ShouldNotBe(root.ServiceProvider);
    }

    [Fact]
    public void ShouldResolveScopedServiceConsistentlyWithinSameAmbientScope()
    {
        using var root = Manager.Begin();
        using var child = Manager.Begin();
        using var subRoot = Manager.Begin(
            new UnitOfWorkOptions(transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew));

        var first = root.ServiceProvider.GetRequiredService<ScopedImp>();
        var second = child.ServiceProvider.GetRequiredService<ScopedImp>();
        var third = subRoot.ServiceProvider.GetRequiredService<ScopedImp>();

        first.ShouldBeSameAs(second);
        second.ShouldBeSameAs(third);
    }

    // CancellationToken

    [Fact]
    public void ShouldUseCancellationTokenNoneWhenNotProvidedAndNoParent()
    {
        using var uow = Manager.Begin();

        uow.CancellationToken.ShouldBe(CancellationToken.None);
    }

    [Fact]
    public void ShouldUseProvidedCancellationTokenWhenBeginCalledWithCancellationToken()
    {
        using var cts = new CancellationTokenSource();

        using var uow = Manager.Begin(cancellationToken: cts.Token);

        uow.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public void ShouldUseParentCancellationTokenWhenChildBegunWithoutExplicitToken()
    {
        using var parentCts = new CancellationTokenSource();

        using var root = Manager.Begin(cancellationToken: parentCts.Token);
        using var child = Manager.Begin();

        child.CancellationToken.ShouldBe(parentCts.Token);
    }

    [Fact]
    public void ShouldUseProvidedTokenWhenParentHasNoToken()
    {
        using var cts = new CancellationTokenSource();

        using var root = Manager.Begin();
        using var child = Manager.Begin(cancellationToken: cts.Token);

        child.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public void ShouldLinkParentAndProvidedTokensWhenBothPresentAndDistinct()
    {
        using var parentCts = new CancellationTokenSource();
        using var childCts = new CancellationTokenSource();

        using var root = Manager.Begin(cancellationToken: parentCts.Token);
        using var child = Manager.Begin(cancellationToken: childCts.Token);

        child.CancellationToken.ShouldNotBe(parentCts.Token);
        child.CancellationToken.ShouldNotBe(childCts.Token);
        child.CancellationToken.CanBeCanceled.ShouldBeTrue();
    }

    [Fact]
    public void ShouldCancelLinkedTokenWhenParentTokenCancelled()
    {
        using var parentCts = new CancellationTokenSource();
        using var childCts = new CancellationTokenSource();

        using var root = Manager.Begin(cancellationToken: parentCts.Token);
        using var child = Manager.Begin(cancellationToken: childCts.Token);

        parentCts.Cancel();

        child.CancellationToken.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public void ShouldCancelLinkedTokenWhenProvidedTokenCancelled()
    {
        using var parentCts = new CancellationTokenSource();
        using var childCts = new CancellationTokenSource();

        using var root = Manager.Begin(cancellationToken: parentCts.Token);
        using var child = Manager.Begin(cancellationToken: childCts.Token);

        childCts.Cancel();

        child.CancellationToken.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public void ShouldReuseParentTokenWhenProvidedTokenEqualsParentToken()
    {
        using var parentCts = new CancellationTokenSource();

        using var root = Manager.Begin(cancellationToken: parentCts.Token);
        using var child = Manager.Begin(cancellationToken: parentCts.Token);

        child.CancellationToken.ShouldBe(parentCts.Token);
    }

    [Fact]
    public void ShouldUseParentCancellationTokenForSubRootWithoutExplicitToken()
    {
        using var parentCts = new CancellationTokenSource();

        using var root = Manager.Begin(cancellationToken: parentCts.Token);
        using var subRoot = Manager.Begin(
            new UnitOfWorkOptions(transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew));

        subRoot.CancellationToken.ShouldBe(parentCts.Token);
    }
}

public class UnitOfWorkManagerFixture : IntegrationTest
{
    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<UnitOfWorkOptions>(options => { options.Timeout = TimeSpan.FromSeconds(30); });

        return Task.CompletedTask;
    }
}

file class ScopedImp : IScopedService {}