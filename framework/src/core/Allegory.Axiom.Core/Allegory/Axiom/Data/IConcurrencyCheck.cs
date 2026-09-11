namespace Allegory.Axiom.Data;

public interface IConcurrencyCheck
{
    uint Revision { get; set; }
}