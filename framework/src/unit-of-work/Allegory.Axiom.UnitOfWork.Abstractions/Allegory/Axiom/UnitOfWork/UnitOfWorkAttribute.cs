using System;
using System.Data;

namespace Allegory.Axiom.UnitOfWork;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class UnitOfWorkAttribute : Attribute
{
    public UnitOfWorkAttribute() {}

    public UnitOfWorkAttribute(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public UnitOfWorkAttribute(UnitOfWorkTransactionBehavior transactionBehavior)
    {
        TransactionBehavior = transactionBehavior;
    }

    public UnitOfWorkAttribute(IsolationLevel isolationLevel)
    {
        IsolationLevel = isolationLevel;
    }

    public UnitOfWorkAttribute(int timeoutMilliseconds)
    {
        Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public UnitOfWorkAttribute(
        UnitOfWorkTransactionBehavior transactionBehavior,
        IsolationLevel isolationLevel)
    {
        TransactionBehavior = transactionBehavior;
        IsolationLevel = isolationLevel;
    }

    public UnitOfWorkAttribute(
        UnitOfWorkTransactionBehavior transactionBehavior,
        int timeoutMilliseconds)
    {
        TransactionBehavior = transactionBehavior;
        Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public UnitOfWorkAttribute(
        IsolationLevel isolationLevel,
        int timeoutMilliseconds)
    {
        IsolationLevel = isolationLevel;
        Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public UnitOfWorkAttribute(
        UnitOfWorkTransactionBehavior transactionBehavior,
        IsolationLevel isolationLevel,
        int timeoutMilliseconds)
    {
        TransactionBehavior = transactionBehavior;
        IsolationLevel = isolationLevel;
        Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public UnitOfWorkTransactionBehavior? TransactionBehavior { get; }
    public IsolationLevel? IsolationLevel { get; }
    public TimeSpan? Timeout { get; }
    public bool IsEnabled { get; } = true;
}