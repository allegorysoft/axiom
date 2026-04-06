using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkOptions
{
    public IsolationLevel? IsolationLevel { get; set; }
    public UnitOfWorkTransactionBehavior TransactionBehavior { get; set; }
    public TimeSpan? Timeout { get; set; }
}