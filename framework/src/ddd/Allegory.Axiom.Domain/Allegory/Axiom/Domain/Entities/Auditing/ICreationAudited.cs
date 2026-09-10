using System;

namespace Allegory.Axiom.Domain.Entities.Auditing;

public interface ICreationAudited : ICreationAudited<string>;

public interface ICreationAudited<TUserKey>
{
    DateTime CreatedAt { get; }
    TUserKey? CreatedBy { get; }
}