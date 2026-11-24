using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Security.Encryption;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Setting;

public class SettingValueManager : ISettingValueManager, ISingletonService
{
    public SettingValueManager(
        IServiceProvider serviceProvider,
        IOptions<SettingValueOptions> options,
        ISettingDefinitionStore settingDefinitionStore,
        IStringEncryptionService stringEncryptionService)
    {
        SettingDefinitionStore = settingDefinitionStore;
        StringEncryptionService = stringEncryptionService;

        FillContributors(serviceProvider, options);
    }

    protected ImmutableArray<ISettingValueContributor> Contributors { get; private set; }
    protected ConcurrentDictionary<SettingDefinition, SettingValue?> DefaultValues { get; } = new();
    protected ISettingDefinitionStore SettingDefinitionStore { get; }
    protected IStringEncryptionService StringEncryptionService { get; }

    public virtual async ValueTask<T?> FindAsync<T>(string name)
    {
        var result = await FindAsync(name);
        return result is null ? default : result.Get<T>();
    }

    public virtual async ValueTask<T?> FindAsync<T>(SettingDefinition definition)
    {
        var result = await FindAsync(definition);
        return result is null ? default : result.Get<T>();
    }

    public virtual async ValueTask<SettingValue?> FindAsync(string name)
    {
        var definition = await SettingDefinitionStore.FindAsync(name);

        if (definition is null)
        {
            return null;
        }

        return await FindAsync(definition);
    }

    public virtual async ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        foreach (var contributor in Contributors)
        {
            if (definition.Contributors.Count > 0 && !definition.Contributors.Contains(contributor.Name))
            {
                continue;
            }

            var result = await contributor.FindAsync(definition);

            if (result is not null)
            {
                if (definition.IsEncrypted &&
                    contributor.Name != ISettingValueContributor.Configuration)
                {
                    var decrypted = StringEncryptionService.Decrypt(result.Value.GetRawText());
                    var value = JsonElement.Parse(decrypted);
                    return new SettingValue(value);
                }

                return result;
            }
        }

        return DefaultValues.GetOrAdd(
            definition,
            static d => d.DefaultValue.HasValue ? new SettingValue(d.DefaultValue.Value) : null);
    }

    private void FillContributors(IServiceProvider serviceProvider, IOptions<SettingValueOptions> options)
    {
        var builder = ImmutableArray.CreateBuilder<ISettingValueContributor>();

        foreach (var contributor in options.Value.Contributors.Reverse())
        {
            builder.Add(
                (ISettingValueContributor) ActivatorUtilities.CreateInstance(serviceProvider, contributor));
        }

        Contributors = builder.ToImmutable();
    }
}