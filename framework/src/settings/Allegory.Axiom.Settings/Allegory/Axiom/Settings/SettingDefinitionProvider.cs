using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Settings;

public class SettingDefinitionProvider : ISettingDefinitionProvider, ISingletonService
{
    public SettingDefinitionProvider(IEnumerable<ISettingDefinitionContributor> contributors)
    {
        Contributors = contributors.ToImmutableList();

        LazySettings = new Lazy<FrozenDictionary<string, SettingDefinition>>(
            Initialize,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public FrozenDictionary<string, SettingDefinition> Settings => LazySettings.Value;

    protected ImmutableList<ISettingDefinitionContributor> Contributors { get; }
    protected Lazy<FrozenDictionary<string, SettingDefinition>> LazySettings { get; }

    protected virtual FrozenDictionary<string, SettingDefinition> Initialize()
    {
        var context = new SettingDefinitionContext();

        foreach (var contributor in Contributors)
        {
            contributor.Contribute(context);
        }

        foreach (var contributor in Contributors)
        {
            contributor.PostContribute(context);
        }

        return context.Settings.ToFrozenDictionary();
    }
}