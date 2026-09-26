using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public interface IDbContextProvider<TContext> where TContext : DbContext
{
    ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default);
}