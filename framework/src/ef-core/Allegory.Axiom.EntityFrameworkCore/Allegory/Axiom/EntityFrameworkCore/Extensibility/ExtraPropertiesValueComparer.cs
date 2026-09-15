using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Allegory.Axiom.EntityFrameworkCore.Extensibility;

internal sealed class ExtraPropertiesValueComparer() :
    ValueComparer<IDictionary<string, object?>>(
        (left, right) => ExtraPropertiesJsonSerializer.AreEqual(left, right),
        static value => ExtraPropertiesJsonSerializer.GetHashCode(value),
        static value => ExtraPropertiesJsonSerializer.Clone(value));