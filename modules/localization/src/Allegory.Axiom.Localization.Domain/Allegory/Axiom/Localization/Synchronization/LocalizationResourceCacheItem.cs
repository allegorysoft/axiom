using System.Collections.Immutable;
using System.ComponentModel;

namespace Allegory.Axiom.Localization;

[ImmutableObject(true)]
public sealed class LocalizationResourcesCacheItem
{
    public ImmutableArray<LocalizationResourceProjection> Items { get; set; }
}