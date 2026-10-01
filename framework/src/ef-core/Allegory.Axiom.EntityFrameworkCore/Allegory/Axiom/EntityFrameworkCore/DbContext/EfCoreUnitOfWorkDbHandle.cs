using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.EntityFrameworkCore;

public class EfCoreUnitOfWorkDbHandle<TContext> : UnitOfWorkDbHandle<TContext> where TContext : DbContext
{
    public EfCoreUnitOfWorkDbHandle(TContext handle, bool isLazyTransactional = false) : base(handle)
    {
        IsLazyTransactional = isLazyTransactional;
    }

    public EfCoreUnitOfWorkDbHandle(TContext handle, IDbContextTransaction? transaction) : base(handle)
    {
        Transaction = transaction;
    }

    public bool IsLazyTransactional { get; protected set; }
    public IDbContextTransaction? Transaction { get; protected set; }

    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await TryBeginTransactionAsync(cancellationToken);
        await Handle.SaveChangesAsync(cancellationToken);
    }

    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Transaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask;
    }

    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Transaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask;
    }

    public override void Dispose()
    {
        Transaction?.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        if (Transaction != null)
        {
            await Transaction.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    protected virtual async Task TryBeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsLazyTransactional || Transaction != null)
        {
            return;
        }

        try
        {
            Transaction = await Handle.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (NotSupportedException e)
        {
            var logger = UnitOfWork.ServiceProvider.GetRequiredService<ILogger<UnitOfWorkDbHandle>>();
            logger.LogWarning(e, "Transaction not supported for {DbContext}", typeof(TContext));
            IsLazyTransactional = false;
        }
    }
}