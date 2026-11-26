using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using Xunit;

namespace Allegory.Axiom.Setting;

public class Test(IntegrationTestFixture fixture) : IClassFixture<IntegrationTestFixture>
{
    protected ISettingDefinitionProvider Provider => fixture.Service<ISettingDefinitionProvider>();
    protected HybridCache Cache => fixture.Service<HybridCache>();

    protected ISettingValueManager ValueManager => fixture.Service<ISettingValueManager>();

    [Fact]
    public async Task T()
    {
        var value = await ValueManager.FindAsync<SmtpSetting>(SmtpSetting.Name);

        var value2 = await ValueManager.FindAsync(SmtpSetting.Name);
    }
}

public class TestSettingDefinitionContributor : ISettingDefinitionContributor, ISingletonService
{
    public void Contribute(SettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(
            SmtpSetting.Name,
            isEncrypted: true));
    }
}

public record SmtpSetting(string Host, int Port, bool UseSsl, SmtpChildSetting Child)
{
    public const string Name = "SMTP";
}

public record SmtpChildSetting(string ChildName, int ChildPort);