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

            await using (var child = Manager.Begin(UnitOfWorkOptions.RequiresNew))
            {
                Manager.Current.ShouldBe(child);
            }

            Manager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public async Task ShouldIsolateUnitOfWorkContextAcrossConcurrentExecutionContexts()
    {
        // Each call to `Manager.Begin` creates a *new* `AsyncLocalContext<>` instance and assigns
        // it to the AsyncLocal slot for the current execution context. Because AsyncLocal writes
        // don't flow back up to the parent, a `Begin` inside a child Task (see `Job` below) only
        // replaces the slot's value within that child's execution context, it never touches the
        // parent's.
        //
        // Disposal is different: `Dispose`/`DisposeAsync` doesn't create a new context, it mutates
        // the `Context` property on the *same* `AsyncLocalContext` instance that both parent and
        // child are (independently) pointing at. That mutation is visible to anyone holding a
        // reference to that instance, regardless of which execution context they're in.
        //
        // So: a concurrently running task can safely `Begin` its own child unit of work without
        // corrupting the caller's ambient state, but disposing a unit of work still correctly
        // restores its parent everywhere that instance is observed.

        await using (var root = Manager.Begin())
        {
            var rootSignal = new TaskCompletionSource();
            var childSignal = new TaskCompletionSource();
            var task = Job(rootSignal, childSignal, root);
            await rootSignal.Task; // Wait for Job to begin its own child unit of work

            // Job's `Begin` ran in its own execution context, so it created its own
            // AsyncLocalContext instance there. It never mutated ours, so `Current` is
            // unaffected here.
            Manager.Current.ShouldBe(root);

            childSignal.SetResult();
            await task; // Job's `child.DisposeAsync` restores Job's own context to `root`
            Manager.Current.ShouldBe(root);
        }

        return;

        async Task Job(TaskCompletionSource rootSignal, TaskCompletionSource childSignal, IUnitOfWork root)
        {
            Manager.Current.ShouldBe(root);
            await using var child = Manager.Begin();
            rootSignal.SetResult();
            Manager.Current.ShouldBe(child);
            Manager.RequiredCurrent.Parent.ShouldBe(root);
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

            using (var subRoot = Manager.Begin(UnitOfWorkOptions.RequiresNew))
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
        using (var root = Manager.Begin(UnitOfWorkOptions.RequiresNew))
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
        using (var root = Manager.Begin(UnitOfWorkOptions.Suppress))
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = Manager.Begin(UnitOfWorkOptions.Suppress))
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

            using (var subRoot = Manager.Begin(UnitOfWorkOptions.Suppress))
            {
                Manager.Current.ShouldBe(subRoot);
                Manager.Current.ShouldBeOfType<UnitOfWork>();
                Manager.Current.Parent.ShouldBe(root);
            }
        }

        // Suppress, Required
        using (var root = Manager.Begin(UnitOfWorkOptions.Suppress))
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
        using (var root = Manager.Begin(UnitOfWorkOptions.RequiresNew))
        {
            Manager.Current.ShouldBe(root);
            Manager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = Manager.Begin(UnitOfWorkOptions.Suppress))
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
        var options = fixture.Service<IOptions<UnitOfWorkDefaultOptions>>().Value;

        using var uow = Manager.Begin();

        Manager.RequiredCurrent.Options.ShouldBe(options.Default);
        Manager.RequiredCurrent.Options.Timeout.ShouldBe(options.Timeout);
    }

    [Fact]
    public void ShouldApplyPreferredOptionsWhenPreferredOptionsNotNull()
    {
        var preferred = new UnitOfWorkOptions(timeout: TimeSpan.FromMinutes(1));
        using var uow = Manager.Begin(preferred);

        Manager.RequiredCurrent.Options.Timeout.ShouldBe(preferred.Timeout);
        Manager.RequiredCurrent.Options.ShouldBeSameAs(preferred);
    }

    [Fact]
    public void ShouldFallbackDefaultOptionsWhenPreferredOptionsPropertyIsNull()
    {
        var options = fixture.Service<IOptions<UnitOfWorkDefaultOptions>>().Value;

        var preferred = new UnitOfWorkOptions(isolationLevel: IsolationLevel.ReadUncommitted);
        using var uow = Manager.Begin(preferred);

        Manager.RequiredCurrent.Options.IsolationLevel.ShouldBe(preferred.IsolationLevel);
        Manager.RequiredCurrent.Options.ShouldBeSameAs(preferred);
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

        using var subRoot = Manager.Begin(UnitOfWorkOptions.RequiresNew);

        subRoot.ServiceProvider.ShouldBeSameAs(root.ServiceProvider);
    }

    [Fact]
    public void ShouldUseExplicitServiceProviderForSubRootWhenProvided()
    {
        var customProvider = fixture.Service<IServiceProvider>();
        using var root = Manager.Begin();

        using var subRoot = Manager.Begin(
            UnitOfWorkOptions.RequiresNew,
            serviceProvider: customProvider);

        subRoot.ServiceProvider.ShouldBeSameAs(customProvider);
        subRoot.ServiceProvider.ShouldNotBe(root.ServiceProvider);
    }

    [Fact]
    public void ShouldResolveScopedServiceConsistentlyWithinSameAmbientScope()
    {
        using var root = Manager.Begin();
        using var child = Manager.Begin();
        using var subRoot = Manager.Begin(UnitOfWorkOptions.RequiresNew);

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
        using var subRoot = Manager.Begin(UnitOfWorkOptions.RequiresNew);

        subRoot.CancellationToken.ShouldBe(parentCts.Token);
    }
}

public class UnitOfWorkManagerFixture : IntegrationTest
{
    protected override Task ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<UnitOfWorkDefaultOptions>(options => { options.Timeout = TimeSpan.FromSeconds(30); });

        return Task.CompletedTask;
    }
}

file class ScopedImp : IScopedService {}