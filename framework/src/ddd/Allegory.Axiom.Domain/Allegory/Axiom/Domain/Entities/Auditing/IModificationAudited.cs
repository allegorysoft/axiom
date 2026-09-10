using System;

namespace Allegory.Axiom.Domain.Entities.Auditing;

public interface IModificationAudited : IModificationAudited<string>;

public interface IModificationAudited<TUserKey>
{
    DateTime? ModifiedAt { get; }
    TUserKey? ModifiedBy { get; }
}