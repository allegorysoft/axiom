using System;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.EntityFrameworkCore;

public class EfCoreUnitOfWorkDbHandle<TContext>(
    TContext handle,
    bool isLazyTransactional = false) :
    UnitOfWorkDbHandle<TContext>(handle)
    where TContext : DbContext
{
    public bool IsLazyTransactional { get; protected set; } = isLazyTransactional;

    public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await TryBeginTransactionAsync(cancellationToken);
        await Handle.SaveChangesAsync(cancellationToken);
    }

    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Handle.Database.CurrentTransaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask;
    }

    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Handle.Database.CurrentTransaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask;
    }

    public override void Dispose()
    {
        Handle.Database.CurrentTransaction?.Dispose();
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        if (Handle.Database.CurrentTransaction != null)
        {
            await Handle.Database.CurrentTransaction.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    protected virtual async Task TryBeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsLazyTransactional || Handle.Database.CurrentTransaction != null)
        {
            return;
        }

        try
        {
            await Handle.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (NotSupportedException e)
        {
            var logger = UnitOfWork.ServiceProvider.GetRequiredService<ILogger<UnitOfWorkDbHandle>>();
            logger.LogWarning(e, "Transaction not supported for {DbContext}", typeof(TContext));
            IsLazyTransactional = false;
        }
    }
}