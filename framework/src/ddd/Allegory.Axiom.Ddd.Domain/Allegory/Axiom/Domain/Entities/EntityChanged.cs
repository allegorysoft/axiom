using System;

namespace Allegory.Axiom.Domain.Entities;

public enum EntityChangeType : byte
{
    Created,
    Updated,
    Deleted
}

public sealed record EntityChanged<TEntity>(
    TEntity Entity,
    EntityChangeType ChangeType,
    TEntity? Previous = default)
    where TEntity : IEntity;

// public sealed record EntityChanged<TEntity>(
//     TEntity Entity,
//     EntityChangeType ChangeType,
//     Func<bool, TEntity>? PreviousDelegate = null)
//     where TEntity : IEntity
// {
//     private Lazy<TEntity>? _lazyPrevious;
//
//     public TEntity? Previous
//     {
//         get
//         {
//             if (PreviousDelegate is null)
//                 return default;
//
//             _lazyPrevious ??= new Lazy<TEntity>(PreviousDelegate(false));
//             return _lazyPrevious.Value;
//         }
//     }
// }