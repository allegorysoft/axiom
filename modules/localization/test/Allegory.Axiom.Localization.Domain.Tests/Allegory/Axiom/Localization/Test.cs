using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.Localization;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    [Fact]
    public async Task T()
    {
        var x = fixture.Service<ILocalizationRecordRepository>();

        fixture.BeginAutoCompletingUnitOfWork();

        var list = await x.GetListAsync();

        var lang = new LocalizationRecord("a", "en");
        var localizationStore = fixture.Service<ILocalizationStore>();

        lang.Translations = localizationStore.Contexts.First().Value.Translations.First().Value.ToDictionary();

        await x.AddAsync(
            lang,
            cancellationToken: TestContext.Current.CancellationToken,
            autoSave: true);

        list = await x.GetListAsync();
    }
}