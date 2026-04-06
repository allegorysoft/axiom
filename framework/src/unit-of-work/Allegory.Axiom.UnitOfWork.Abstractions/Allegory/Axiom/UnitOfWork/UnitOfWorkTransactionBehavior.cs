namespace Allegory.Axiom.UnitOfWork;

/// <summary>
/// Defines the transactional behavior when beginning a unit of work.
/// </summary>
public enum UnitOfWorkTransactionBehavior
{
    /// <summary>
    /// Joins the ambient transaction if one exists. Creates a new transaction if there is no ambient unit of work.
    /// </summary>
    Required,

    /// <summary>
    /// Always creates a new independent transaction regardless of any ambient unit of work.
    /// </summary>
    RequiresNew,

    /// <summary>
    /// Runs without a transaction. Each SaveChangesAsync call is auto-committed immediately and cannot be rolled back.
    /// Useful for operations that must persist regardless of the outcome of an outer transaction.
    /// </summary>
    Suppress
}