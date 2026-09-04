using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

public class UnitOfWorkDefaultOptions
{
    public UnitOfWorkOptions Current { get; } = new();

    public UnitOfWorkTransactionBehavior TransactionBehavior
    {
        get => Current.TransactionBehavior;
        set => Current.TransactionBehavior = value;
    }

    public IsolationLevel? IsolationLevel
    {
        get => Current.IsolationLevel;
        set => Current.IsolationLevel = value;
    }

    public TimeSpan? Timeout
    {
        get => Current.Timeout;
        set => Current.Timeout = value;
    }
}