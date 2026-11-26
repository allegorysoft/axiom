using System.Collections.Immutable;
using System.ComponentModel;

namespace Allegory.Axiom.Localization;

[ImmutableObject(true)]
public sealed class LocalizationCacheItem
{
    public ImmutableArray<LocalizationResourceProjection> Resources { get; init; }
}