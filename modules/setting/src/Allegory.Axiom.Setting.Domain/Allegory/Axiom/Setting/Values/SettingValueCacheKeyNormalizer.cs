using Allegory.Axiom.Caching;
using Allegory.Axiom.DependencyInjection;

namespace Allegory.Axiom.Setting;

public class SettingValueCacheKeyNormalizer(ICacheKeyNormalizer keyNormalizer) : ISingletonService
{
    protected ICacheKeyNormalizer KeyNormalizer { get; } = keyNormalizer;

    public virtual string Normalize(string name, SettingValueIdentifier identifier)
    {
        return KeyNormalizer.Normalize($"settings:values:{name}:{identifier.Name}:{identifier.Key ?? "<null>"}");
    }
}