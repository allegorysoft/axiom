using System;
using System.Text.Json;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Setting;

namespace DefaultNamespace;

public class WebApp2SettingDefinitionContributor : ISettingDefinitionContributor, ISingletonService
{
    public void Contribute(SettingDefinitionContext context)
    {
        context.Add(new SettingDefinition("WebApp2-Private", JsonElement.Parse("1")));

        context.Add(SettingDefinition.Create("WebApp2-Bool", true, isPublic: true));
        context.Add(SettingDefinition.Create("WebApp2-String", "string-value", isPublic: true));
        context.Add(SettingDefinition.Create("WebApp2-Datetime", DateTime.MinValue, isPublic: true));
        context.Add(SettingDefinition.Create("WebApp2-Complex", new WebApp2ComplexSetting
        {
            Host = "127.0.0.1",
            Port = 1234
        }, isPublic: true, contributors: ["A", "B"]));
    }
}

public class WebApp2ComplexSetting
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
}