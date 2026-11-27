using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Exceptions;
using Allegory.Axiom.Security.Encryption;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Setting;

public class SettingValueManager : ISettingValueManager, ISingletonService
{
    public SettingValueManager(
        IServiceProvider serviceProvider,
        IOptions<SettingOptions> options,
        ISettingDefinitionStore settingDefinitionStore,
        IStringEncryptionService stringEncryptionService,
        ISettingValueStore settingValueStore)
    {
        SettingDefinitionStore = settingDefinitionStore;
        StringEncryptionService = stringEncryptionService;
        SettingValueStore = settingValueStore;

        FillContributors(serviceProvider, options.Value.SettingValue);
    }

    protected ImmutableArray<ISettingValueContributor> Contributors { get; private set; }
    protected ISettingDefinitionStore SettingDefinitionStore { get; }
    protected IStringEncryptionService StringEncryptionService { get; }
    protected ISettingValueStore SettingValueStore { get; }

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
        var definition = await SettingDefinitionStore.GetAsync(name);
        return await FindAsync(definition);
    }

    public virtual async ValueTask<SettingValue?> FindAsync(SettingDefinition definition)
    {
        foreach (var contributor in Contributors)
        {
            if (!IsContributorAllowed(definition, contributor.Name))
            {
                continue;
            }

            var result = await contributor.FindAsync(definition);

            if (result is null)
            {
                continue;
            }

            return definition.IsEncrypted && contributor.IsEncrypted
                ? new SettingValue(DecryptSettingValue(result))
                : result;
        }

        return null;
    }

    public virtual async Task SetAsync<T>(
        string name,
        SettingValueIdentifier identifier,
        T value,
        bool checkFallbackValue = true)
    {
        var definition = await SettingDefinitionStore.GetAsync(name);
        await SetAsync(definition, identifier, value, checkFallbackValue);
    }

    public virtual async Task SetAsync<T>(
        SettingDefinition definition,
        SettingValueIdentifier identifier,
        T value,
        bool checkFallbackValue = true)
    {
        CheckContributorAllowedToSet(definition, identifier);

        var json = JsonSerializer.Serialize(value);
        var element = JsonElement.Parse(json);

        if (checkFallbackValue &&
            await RemoveIfFallbackValueEqualsAsync(definition, identifier, element))
        {
            return;
        }

        if (definition.IsEncrypted && Contributors.Single(c => c.Name == identifier.Name).IsEncrypted)
        {
            element = JsonSerializer.SerializeToElement(StringEncryptionService.Encrypt(json));
        }

        await SettingValueStore.SetAsync(definition.Name, identifier, new SettingValue(element));
    }

    protected virtual void CheckContributorAllowedToSet(
        SettingDefinition definition,
        SettingValueIdentifier identifier)
    {
        if (!IsContributorAllowed(definition, identifier.Name))
        {
            throw new BusinessException(SettingExceptionCodes.ContributorNotAllowed)
                .AddData("SettingName", definition.Name)
                .AddData("ContributorName", identifier.Name);
        }

        if (identifier.Name is
            DefaultSettingValueContributor.ContributorName or
            ISettingValueContributor.Configuration)
        {
            throw new BusinessException(SettingExceptionCodes.ContributorDoesNotSupportUpdates)
                .AddData("ContributorName", identifier.Name);
        }
    }

    protected virtual async ValueTask<bool> RemoveIfFallbackValueEqualsAsync(
        SettingDefinition definition,
        SettingValueIdentifier identifier,
        JsonElement element)
    {
        foreach (var contributor in Contributors.SkipWhile(c => c.Name != identifier.Name).Skip(1))
        {
            if (!IsContributorAllowed(definition, contributor.Name))
            {
                continue;
            }

            var result = await contributor.FindAsync(definition);

            if (result is null)
            {
                continue;
            }

            if (IsSettingValueEquals(definition, contributor, result, element))
            {
                await SettingValueStore.RemoveAsync(definition.Name, identifier);
                return true;
            }

            return false;
        }

        return false;
    }

    protected virtual bool IsSettingValueEquals(
        SettingDefinition definition,
        ISettingValueContributor contributor,
        SettingValue value,
        JsonElement element)
    {
        var compareValue = value.Value;

        if (definition.IsEncrypted && contributor.IsEncrypted)
        {
            compareValue = DecryptSettingValue(value);
        }

        return JsonElement.DeepEquals(element, compareValue);
    }

    protected virtual bool IsContributorAllowed(SettingDefinition definition, string contributorName)
    {
        return definition.Contributors.Count == 0 ||
               definition.Contributors.Contains(contributorName) ||
               contributorName == DefaultSettingValueContributor.ContributorName;
    }

    protected virtual JsonElement DecryptSettingValue(SettingValue value)
    {
        var decrypted = StringEncryptionService.Decrypt(value.Value.GetString()!);
        return JsonElement.Parse(decrypted);
    }

    private void FillContributors(IServiceProvider serviceProvider, SettingValueOptions options)
    {
        var builder = ImmutableArray.CreateBuilder<ISettingValueContributor>(options.Contributors.Count);
        foreach (var contributor in options.Contributors.Reverse())
        {
            builder.Add(
                (ISettingValueContributor) ActivatorUtilities.CreateInstance(serviceProvider, contributor));
        }

        builder.Add(Activator.CreateInstance<DefaultSettingValueContributor>());
        Contributors = builder.ToImmutable();

        var duplicate = Contributors
            .GroupBy(x => x.Name)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();

        if (duplicate.Length > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate setting value contributor names: {string.Join(", ", duplicate)}");
        }
    }
}