namespace Allegory.Axiom.Domain.Entities.Auditing;

public interface ISoftDelete
{
    bool IsDeleted { get; }
}