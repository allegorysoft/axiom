using System.Collections.Generic;

namespace Allegory.Axiom.Domain.Entities;

public interface IAggregateRoot : IEntity
{
    IReadOnlyList<object> GetLocalEvents();
    void ClearLocalEvents();
    IReadOnlyList<object> GetDistributedEvents();
    void ClearDistributedEvents();
}

public interface IAggregateRoot<TKey> : IAggregateRoot, IEntity<TKey> where TKey : notnull;