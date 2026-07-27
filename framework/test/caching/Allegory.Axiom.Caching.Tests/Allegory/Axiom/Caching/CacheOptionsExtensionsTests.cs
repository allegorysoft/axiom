using System;
using Microsoft.Extensions.Caching.Hybrid;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Caching;

public class CacheOptionsExtensionsTests
{
    [Fact]
    public void ShouldReturnNullWhenNotSet()
    {
        new CacheOptions().ConfigureHybrid.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnSameDelegateAfterSet()
    {
        var options = new CacheOptions();
        Action<HybridCacheOptions> configure = _ => {};

        options.ConfigureHybrid = configure;

        options.ConfigureHybrid.ShouldBeSameAs(configure);
    }

    [Fact]
    public void ShouldOverwritePreviousDelegate()
    {
        var options = new CacheOptions();
        Action<HybridCacheOptions> first = _ => {};
        Action<HybridCacheOptions> second = _ => {};

        options.ConfigureHybrid = first;
        options.ConfigureHybrid = second;

        options.ConfigureHybrid.ShouldBeSameAs(second);
    }

    [Fact]
    public void ShouldCombineDelegatesOnAddAssignment()
    {
        var options = new CacheOptions
        {
            ConfigureHybrid = o =>
            {
                o.MaximumKeyLength = 1;
            }
        };

        options.ConfigureHybrid += o => o.MaximumPayloadBytes = 2;
        options.ConfigureHybrid += o => o.DisableCompression = true;

        var hybridOptions = new HybridCacheOptions();
        options.ConfigureHybrid!.Invoke(hybridOptions);

        hybridOptions.MaximumKeyLength.ShouldBe(1);
        hybridOptions.MaximumPayloadBytes.ShouldBe(2);
        hybridOptions.DisableCompression.ShouldBe(true);
    }

    [Fact]
    public void ShouldNotShareBetweenOptionsInstances()
    {
        var first = new CacheOptions {ConfigureHybrid = _ => {}};
        var second = new CacheOptions();

        first.ConfigureHybrid.ShouldNotBeNull();
        second.ConfigureHybrid.ShouldBeNull();
    }
}