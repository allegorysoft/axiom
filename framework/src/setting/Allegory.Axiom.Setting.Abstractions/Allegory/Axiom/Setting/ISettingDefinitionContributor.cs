namespace Allegory.Axiom.Setting;

public interface ISettingDefinitionContributor
{
    void Contribute(SettingDefinitionContext context);
    void PostContribute(SettingDefinitionContext context) { }
}