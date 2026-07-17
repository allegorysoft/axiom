using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Allegory.Axiom.UnitOfWork;

public static class UnitOfWorkExtensions
{
    extension(IUnitOfWork uow)
    {
        public async Task TryCompleteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await uow.CompleteAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                await uow.TryRollbackAsync(exception, cancellationToken);
                throw;
            }
        }

        public async Task TryRollbackAsync(
            Exception innerException,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await uow.RollbackAsync(cancellationToken);
                if (uow.Activity is not null)
                {
                    uow.Activity.SetStatus(ActivityStatusCode.Error, innerException.Message);
                    uow.Activity.AddException(innerException);
                }
            }
            catch (Exception exception)
            {
                if (uow.Activity is not null)
                {
                    uow.Activity.SetStatus(ActivityStatusCode.Error, "Rollback failed");
                    uow.Activity.AddException(innerException);
                    uow.Activity.AddException(exception);
                }

                throw new AggregateException(exception, innerException);
            }
        }
    }
}