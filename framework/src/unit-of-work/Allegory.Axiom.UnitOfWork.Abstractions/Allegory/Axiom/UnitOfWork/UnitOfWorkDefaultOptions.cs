using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkDefaultOptions
{
    public UnitOfWorkOptions Default { get; } = new();

    public UnitOfWorkTransactionBehavior TransactionBehavior
    {
        get => Default.TransactionBehavior;
        set => Default.TransactionBehavior = value;
    }

    public IsolationLevel? IsolationLevel
    {
        get => Default.IsolationLevel;
        set => Default.IsolationLevel = value;
    }

    public TimeSpan? Timeout
    {
        get => Default.Timeout;
        set => Default.Timeout = value;
    }
}