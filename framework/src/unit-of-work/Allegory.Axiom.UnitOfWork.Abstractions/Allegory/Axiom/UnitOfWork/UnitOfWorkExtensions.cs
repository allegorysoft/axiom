using System;
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
            }
            catch (Exception exception)
            {
                throw new AggregateException(exception, innerException);
            }
        }
    }
}