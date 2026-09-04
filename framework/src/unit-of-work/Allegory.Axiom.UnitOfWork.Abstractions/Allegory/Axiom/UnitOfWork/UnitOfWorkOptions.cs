using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkOptions
{
    public static readonly UnitOfWorkOptions Suppress = new(UnitOfWorkTransactionBehavior.Suppress);
    public static readonly UnitOfWorkOptions Required = new(UnitOfWorkTransactionBehavior.Required);
    public static readonly UnitOfWorkOptions RequiresNew = new(UnitOfWorkTransactionBehavior.RequiresNew);

    public UnitOfWorkOptions() {}

    public UnitOfWorkOptions(
        UnitOfWorkTransactionBehavior? transactionBehavior = null,
        IsolationLevel? isolationLevel = null,
        TimeSpan? timeout = null)
    {
        TransactionBehavior = transactionBehavior ?? UnitOfWorkTransactionBehavior.Required;
        IsolationLevel = isolationLevel;
        Timeout = timeout;
    }

    public UnitOfWorkTransactionBehavior TransactionBehavior { get; set; }
    public IsolationLevel? IsolationLevel { get; set; }
    public TimeSpan? Timeout { get; set; }
}