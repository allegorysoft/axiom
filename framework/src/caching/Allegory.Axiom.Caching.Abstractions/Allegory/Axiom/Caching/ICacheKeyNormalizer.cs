namespace Allegory.Axiom.Caching;

public interface ICacheKeyNormalizer
{
    /// <summary>
    /// Normalizes a key without a cache type descriptor.
    /// Specify the cache context in the key.
    /// </summary>
    string Normalize(string key, bool isTenantOwned = false);

    /// <summary>
    /// Normalizes a key using the specified cache type's descriptor.
    /// </summary>
    string Normalize<TCache>(string key);

    /// <summary>
    /// Normalizes a key using the provided cache descriptor.
    /// </summary>
    string Normalize(string key, CacheTypeDescriptor descriptor);

    /// <summary>
    /// Gets the descriptor associated with the specified cache type.
    /// </summary>
    CacheTypeDescriptor GetDescriptor<TCache>();
}