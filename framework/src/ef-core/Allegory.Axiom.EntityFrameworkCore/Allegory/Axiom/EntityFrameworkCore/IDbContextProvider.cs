using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Data;
using Allegory.Axiom.MultiTenancy;
using Allegory.Axiom.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.EntityFrameworkCore;

public interface IDbContextProvider<TContext> where TContext : DbContext
{
    IUnitOfWorkManager UnitOfWorkManager { get; }
    ITenantContextAccessor TenantContextAccessor { get; }
    AxiomDbContextOptions<TContext> Options { get; }
    IConnectionStringProvider ConnectionStringProvider { get; }

    ValueTask<TContext> GetAsync(CancellationToken cancellationToken = default);
}