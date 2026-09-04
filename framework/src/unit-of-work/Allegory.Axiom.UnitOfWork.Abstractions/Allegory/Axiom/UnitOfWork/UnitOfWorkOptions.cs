using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkOptions(
    UnitOfWorkTransactionBehavior? transactionBehavior = null,
    IsolationLevel? isolationLevel = null,
    TimeSpan? timeout = null)
{
    public static readonly UnitOfWorkOptions Suppress = new(UnitOfWorkTransactionBehavior.Suppress);
    public static readonly UnitOfWorkOptions Required = new(UnitOfWorkTransactionBehavior.Required);
    public static readonly UnitOfWorkOptions RequiresNew = new(UnitOfWorkTransactionBehavior.RequiresNew);

    public UnitOfWorkTransactionBehavior TransactionBehavior { get; internal set; } = transactionBehavior ?? UnitOfWorkTransactionBehavior.Required;
    public IsolationLevel? IsolationLevel { get; internal set; } = isolationLevel;
    public TimeSpan? Timeout { get; internal set; } = timeout;
}