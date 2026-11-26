using System;
using System.Text.Json;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Setting;

namespace DefaultNamespace;

public class ConsoleAppSettingDefinitionContributor : ISettingDefinitionContributor, ISingletonService
{
    public void Contribute(SettingDefinitionContext context)
    {
        context.Add(new SettingDefinition("Private", JsonElement.Parse("1")));

        context.Add(SettingDefinition.Create("Bool", true, isPublic: true));
        context.Add(SettingDefinition.Create("String", "string-value", isPublic: true));
        context.Add(SettingDefinition.Create("Datetime", DateTime.MinValue, isPublic: true));
        context.Add(SettingDefinition.Create("Complex", new ComplexSetting
        {
            Host = "127.0.0.1",
            Port = 1234
        }, isPublic: true, contributors: ["A", "B"]));
    }
}

public class ComplexSetting
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
}