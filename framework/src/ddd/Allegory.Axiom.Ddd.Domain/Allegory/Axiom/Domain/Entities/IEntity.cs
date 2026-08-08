namespace Allegory.Axiom.Domain.Entities;

public interface IEntity
{
    object[] GetKeys();
}

public interface IEntity<TKey> : IEntity where TKey : notnull
{
    TKey Id { get; }

    object[] IEntity.GetKeys() => [Id];
}