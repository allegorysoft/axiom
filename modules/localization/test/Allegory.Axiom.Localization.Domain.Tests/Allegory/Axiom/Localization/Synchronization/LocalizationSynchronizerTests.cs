using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Allegory.Axiom.Localization;

public class LocalizationSynchronizerTests(
    IntegrationTestFixture fixture) :
    IClassFixture<IntegrationTestFixture>
{
    protected ILocalizationRecordRepository Repository => fixture.Service<ILocalizationRecordRepository>();
    protected LocalizationSynchronizer Synchronizer => fixture.Service<LocalizationSynchronizer>();
    protected ILocalizationProvider Provider => fixture.Service<ILocalizationProvider>();

    [Fact]
    public async Task ShouldSaveResourcesAndTranslationsOnSynchronizeAsync()
    {
        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var resourcesBefore = await Repository.GetResourceListAsync(CancellationToken.None);
            resourcesBefore.ShouldBeEmpty();
        });

        var context = new LocalizationContext
        {
            DefaultCulture = "en",
            Translations = new ConcurrentDictionary<string, FrozenDictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string>
                    {
                        ["Hello"] = "Hello", ["Welcome"] = "Welcome"
                    }
                    .ToFrozenDictionary(),
                ["tr"] = new Dictionary<string, string>
                    {
                        ["Hello"] = "Merhaba", ["Welcome"] = "Hoş geldiniz"
                    }
                    .ToFrozenDictionary(),
            }
        };
        Provider.Contexts.TryAdd("TestResource", context);

        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var resourcesAfter = await Repository.GetResourceListAsync(CancellationToken.None);

            var resource = resourcesAfter.Single(r => r.Name == "TestResource");
            resource.Name.ShouldBe("TestResource");
            resource.DefaultCulture.ShouldBe("en");
            resource.Cultures.ShouldBe(["en", "tr"], ignoreOrder: true);
        });
    }

    [Fact]
    public async Task ShouldUpdateExistingTranslationsOnReSynchronize()
    {
        var context = new LocalizationContext
        {
            DefaultCulture = "en",
            Translations = new ConcurrentDictionary<string, FrozenDictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string> {["Key1"] = "Value1"}.ToFrozenDictionary(),
            }
        };
        Provider.Contexts.TryAdd("Resource1", context);
        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        context.Translations["en"] = new Dictionary<string, string>
        {
            ["Key1"] = "UpdatedValue1", ["Key2"] = "NewValue"
        }.ToFrozenDictionary();
        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        await fixture.RunInUnitOfWorkAsync(async _ =>
        {
            var record = await Repository.FindAsync(r => r.Name == "Resource1" && r.Culture == "en");
            record.ShouldNotBeNull();
            record.Translations["Key1"].ShouldBe("UpdatedValue1");
            record.Translations["Key2"].ShouldBe("NewValue");
        });
    }

    [Fact]
    public async Task ShouldReloadContextsFromDatabaseOnReloadAsync()
    {
        Provider.Contexts.TryAdd("Resource1", new LocalizationContext
        {
            DefaultCulture = "en",
            Translations = new ConcurrentDictionary<string, FrozenDictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string> {["Key"] = "Value"}.ToFrozenDictionary(),
            }
        });
        await Synchronizer.SynchronizeAsync(TestContext.Current.CancellationToken);

        Provider.Contexts.Clear();
        await Synchronizer.ReloadAsync(TestContext.Current.CancellationToken);

        Provider.Contexts.ShouldContainKey("Resource1");
        var reloaded = Provider.Contexts["Resource1"];
        reloaded.DefaultCulture.ShouldBe("en");
        reloaded.Translations["en"]["Key"].ShouldBe("Value");
    }
}