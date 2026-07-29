using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkOptions
{
    public static readonly UnitOfWorkOptions SuppressedTransaction = new(UnitOfWorkTransactionBehavior.Suppress);

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

    public IsolationLevel? IsolationLevel { get; set; }
    public UnitOfWorkTransactionBehavior TransactionBehavior { get; set; }
    public TimeSpan? Timeout { get; set; }
}