using System.Threading.Tasks;
using Allegory.Axiom.Disposables;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Data.Filtering;

public class FilterSwitchTests(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected IFilterSwitch Switch => fixture.Service<IFilterSwitch>();

    [Fact]
    public void ShouldBeEnabledByDefault()
    {
        Switch.IsEnabled<TestFilter>().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldUseConfiguredDefault()
    {
        var provider = await fixture.CreateServiceProviderAsync(b =>
        {
            b.Services.Configure<FilterSwitchOptions>(o => { o.Defaults[typeof(TestFilter)] = false; });
        });

        var filterSwitch = provider.GetRequiredService<IFilterSwitch>();

        filterSwitch.IsEnabled<TestFilter>().ShouldBeFalse();
    }

    [Fact]
    public void ShouldEnableFilter()
    {
        Switch.Disable<TestFilter>();

        Switch.IsEnabled<TestFilter>().ShouldBeFalse();

        Switch.Enable<TestFilter>();

        Switch.IsEnabled<TestFilter>().ShouldBeTrue();
    }

    [Fact]
    public void ShouldDisableFilter()
    {
        Switch.IsEnabled<TestFilter>().ShouldBeTrue();

        Switch.Disable<TestFilter>();

        Switch.IsEnabled<TestFilter>().ShouldBeFalse();
    }

    [Fact]
    public void ShouldRestorePreviousState()
    {
        Switch.IsEnabled<TestFilter>().ShouldBeTrue();

        using (Switch.Disable<TestFilter>())
        {
            Switch.IsEnabled<TestFilter>().ShouldBeFalse();

            using (Switch.Enable<TestFilter>())
            {
                Switch.IsEnabled<TestFilter>().ShouldBeTrue();
            }

            Switch.IsEnabled<TestFilter>().ShouldBeFalse();
        }

        Switch.IsEnabled<TestFilter>().ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnEmptyDisposableWhenStateDoesNotChange()
    {
        using (var enabled1 = Switch.Enable<TestFilter>())
        {
            enabled1.ShouldNotBeOfType<EmptyDisposable>();

            using (var enabled2 = Switch.Enable<TestFilter>())
            {
                enabled2.ShouldBeOfType<EmptyDisposable>();

                using (var disabled1 = Switch.Disable<TestFilter>())
                {
                    disabled1.ShouldNotBeOfType<EmptyDisposable>();

                    using (var disabled2 = Switch.Disable<TestFilter>())
                    {
                        disabled2.ShouldBeOfType<EmptyDisposable>();
                    }
                }
            }
        }
    }

    [Fact]
    public async Task ShouldPropagateStateBetweenAsyncFlows()
    {
        Switch.Disable<TestFilter>();

        await Task.Run(() =>
            {
                Switch.IsEnabled<TestFilter>().ShouldBeFalse();
                Switch.Enable<TestFilter>();
                Switch.IsEnabled<TestFilter>().ShouldBeTrue();
            },
            TestContext.Current.CancellationToken);

        Switch.IsEnabled<TestFilter>().ShouldBeFalse();
    }

    [Fact]
    public void ShouldMaintainIndependentStateForDifferentFilters()
    {
        Switch.Disable<TestFilter>();

        Switch.IsEnabled<TestFilter>().ShouldBeFalse();
        Switch.IsEnabled<AnotherTestFilter>().ShouldBeTrue();
    }
}

file class TestFilter;

file class AnotherTestFilter;