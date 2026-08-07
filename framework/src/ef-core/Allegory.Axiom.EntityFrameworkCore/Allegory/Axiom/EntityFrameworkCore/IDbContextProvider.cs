using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public interface IDbContextProvider<TContext> where TContext : DbContext
{
    IUnitOfWorkManager UnitOfWorkManager { get; }
    ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default);
}