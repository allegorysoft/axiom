using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManagerTests : IntegrationTestBase
{
    protected IUnitOfWorkManager UnitOfWorkManager => Service<IUnitOfWorkManager>();

    protected override ValueTask ConfigureAsync(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<UnitOfWorkOptions>(options =>
        {
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        return ValueTask.CompletedTask;
    }

    [Fact]
    public void ShouldCreateUnitOfWork()
    {
        using (var root = UnitOfWorkManager.Begin())
        {
            UnitOfWorkManager.Current.ShouldNotBeNull();
            UnitOfWorkManager.Current.ShouldBe(root);
        }

        UnitOfWorkManager.Current.ShouldBeNull();
    }

    [Fact]
    public void ShouldCreateChildUnitOfWorkWhenParentExists()
    {
        using (var root = UnitOfWorkManager.Begin())
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = UnitOfWorkManager.Begin())
            {
                UnitOfWorkManager.Current.ShouldBe(child);
                UnitOfWorkManager.Current.ShouldBeOfType<ChildUnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldRestoreParentUnitOfWorkAfterChildUnitOfWorkDisposed()
    {
        using (var root = UnitOfWorkManager.Begin())
        {
            UnitOfWorkManager.Current.ShouldBe(root);

            using (var child = UnitOfWorkManager.Begin())
            {
                UnitOfWorkManager.Current.ShouldBe(child);
            }

            UnitOfWorkManager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public void ShouldUseParentPropertiesWhenUnitOfWorkIsChild()
    {
        using (var root = UnitOfWorkManager.Begin())
        {
            root.Items["key"] = "value";
            using (var child = UnitOfWorkManager.Begin())
            {
                UnitOfWorkManager.Current!.Items["key"].ShouldBe("value");
                root.Items.ShouldBe(child.Items);
            }
        }
    }

    [Fact]
    public void ShouldCreateSubRootUnitOfWorkWhenTransactionBehaviorIsRequiresNew()
    {
        using (var root = UnitOfWorkManager.Begin())
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
            {
                UnitOfWorkManager.Current.ShouldBe(subRoot);
                UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();
            }

            UnitOfWorkManager.Current.ShouldBe(root);
        }
    }

    [Fact]
    public void ShouldCreateChildUnitOfWorkWhenTransactionBehaviorCompatible()
    {
        // Required, Required
        using (var root = UnitOfWorkManager.Begin())
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = UnitOfWorkManager.Begin())
            {
                UnitOfWorkManager.Current.ShouldBe(child);
                UnitOfWorkManager.Current.ShouldBeOfType<ChildUnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }

        // RequiresNew, Required
        using (var root = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = UnitOfWorkManager.Begin())
            {
                UnitOfWorkManager.Current.ShouldBe(child);
                UnitOfWorkManager.Current.ShouldBeOfType<ChildUnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }

        // Suppress, Suppress
        using (var root = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var child = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                UnitOfWorkManager.Current.ShouldBe(child);
                UnitOfWorkManager.Current.ShouldBeOfType<ChildUnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldCreateSubRootUnitOfWorkWhenTransactionBehaviorIncompatible()
    {
        // Required, Suppress
        using (var root = UnitOfWorkManager.Begin())
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                UnitOfWorkManager.Current.ShouldBe(subRoot);
                UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }

        // Suppress, Required
        using (var root = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = UnitOfWorkManager.Begin())
            {
                UnitOfWorkManager.Current.ShouldBe(subRoot);
                UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }

        // RequiresNew, Suppress
        using (var root = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                   transactionBehavior: UnitOfWorkTransactionBehavior.RequiresNew)))
        {
            UnitOfWorkManager.Current.ShouldBe(root);
            UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();

            using (var subRoot = UnitOfWorkManager.Begin(new UnitOfWorkOptions(
                       transactionBehavior: UnitOfWorkTransactionBehavior.Suppress)))
            {
                UnitOfWorkManager.Current.ShouldBe(subRoot);
                UnitOfWorkManager.Current.ShouldBeOfType<UnitOfWork>();
                UnitOfWorkManager.Current.Parent.ShouldBe(root);
            }
        }
    }

    [Fact]
    public void ShouldApplyDefaultOptionsWhenPreferredOptionsNull()
    {
        var options = Service<IOptions<UnitOfWorkOptions>>().Value;

        using var uow = UnitOfWorkManager.Begin();

        UnitOfWorkManager.Current!.Options.ShouldBe(options);
        UnitOfWorkManager.Current!.Options.Timeout.ShouldBe(options.Timeout);
    }

    [Fact]
    public void ShouldApplyPreferredOptionsWhenPreferredOptionsNotNull()
    {
        var preferred = new UnitOfWorkOptions(timeout: TimeSpan.FromMinutes(1));
        using var uow = UnitOfWorkManager.Begin(preferred);

        UnitOfWorkManager.Current!.Options.ShouldBe(preferred);
        UnitOfWorkManager.Current!.Options.Timeout.ShouldBe(preferred.Timeout);
    }

    [Fact]
    public void ShouldFallbackDefaultOptionsWhenPreferredOptionsPropertyIsNull()
    {
        var options = Service<IOptions<UnitOfWorkOptions>>().Value;

        var preferred = new UnitOfWorkOptions(isolationLevel: IsolationLevel.ReadUncommitted);
        using var uow = UnitOfWorkManager.Begin(preferred);

        UnitOfWorkManager.Current!.Options.ShouldBe(preferred);
        UnitOfWorkManager.Current!.Options.IsolationLevel.ShouldBe(preferred.IsolationLevel);
        UnitOfWorkManager.Current!.Options.Timeout.ShouldBe(options.Timeout);
    }
}