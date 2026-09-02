using System;

namespace Allegory.Axiom.Domain.Entities.Auditing;

public interface IDeletionAudited : IDeletionAudited<string>;

public interface IDeletionAudited<TUserKey> : ISoftDelete
{
    DateTime? DeletedAt { get; }
    TUserKey? DeletedBy { get; }
}