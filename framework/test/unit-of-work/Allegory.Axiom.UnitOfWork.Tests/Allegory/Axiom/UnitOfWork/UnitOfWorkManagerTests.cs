using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkManagerTests : HostedIntegrationTestBase
{
    protected IUnitOfWorkManager Manager => Service<IUnitOfWorkManager>();

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
    public void ShouldUseParentPropertiesWhenUnitOfWorkIsChild()
    {
        using (var root = Manager.Begin())
        {
            root.Items["key"] = "value";
            using (var child = Manager.Begin())
            {
                Manager.Current!.Items["key"].ShouldBe("value");
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

    [Fact]
    public void ShouldApplyDefaultOptionsWhenPreferredOptionsNull()
    {
        var options = Service<IOptions<UnitOfWorkOptions>>().Value;

        using var uow = Manager.Begin();

        Manager.Current!.Options.ShouldBe(options);
        Manager.Current!.Options.Timeout.ShouldBe(options.Timeout);
    }

    [Fact]
    public void ShouldApplyPreferredOptionsWhenPreferredOptionsNotNull()
    {
        var preferred = new UnitOfWorkOptions(timeout: TimeSpan.FromMinutes(1));
        using var uow = Manager.Begin(preferred);

        Manager.Current!.Options.ShouldBe(preferred);
        Manager.Current!.Options.Timeout.ShouldBe(preferred.Timeout);
    }

    [Fact]
    public void ShouldFallbackDefaultOptionsWhenPreferredOptionsPropertyIsNull()
    {
        var options = Service<IOptions<UnitOfWorkOptions>>().Value;

        var preferred = new UnitOfWorkOptions(isolationLevel: IsolationLevel.ReadUncommitted);
        using var uow = Manager.Begin(preferred);

        Manager.Current!.Options.ShouldBe(preferred);
        Manager.Current!.Options.IsolationLevel.ShouldBe(preferred.IsolationLevel);
        Manager.Current!.Options.Timeout.ShouldBe(options.Timeout);
    }
}