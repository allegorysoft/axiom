namespace Allegory.Axiom.Settings;

public interface ISettingDefinitionContributor
{
    void Contribute(SettingDefinitionContext context);
    void PostContribute(SettingDefinitionContext context) { }
}