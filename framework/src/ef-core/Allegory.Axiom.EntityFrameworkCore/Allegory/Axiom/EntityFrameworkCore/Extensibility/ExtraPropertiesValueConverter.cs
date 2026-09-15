using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Allegory.Axiom.EntityFrameworkCore.Extensibility;

internal sealed class ExtraPropertiesValueConverter() :
    ValueConverter<IDictionary<string, object?>, string>(
        static value => ExtraPropertiesJsonSerializer.Serialize(value),
        static value => ExtraPropertiesJsonSerializer.Deserialize(value));
