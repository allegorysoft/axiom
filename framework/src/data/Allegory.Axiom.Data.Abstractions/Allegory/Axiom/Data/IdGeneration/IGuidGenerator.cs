using System;

namespace Allegory.Axiom.Data.IdGeneration;

public interface IGuidGenerator
{
    Guid Create(GuidGenerationType? type = null);
}