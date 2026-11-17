using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Localization;

public class EfCoreLocalizationRecordRepository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, LocalizationRecord, Guid>(serviceProvider),
    ILocalizationRecordRepository
    where TDbContext : DbContext
{
    public virtual async Task<ImmutableArray<LocalizationResourceProjection>> GetResourceListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        var queryable = await GetQueryableAsync(false, cancellationToken);

        var resources = await queryable.Select(e => new
        {
            e.Name,
            e.IsDefault,
            e.Culture
        }).ToListAsync(cancellationToken: cancellationToken);

        return resources
            .GroupBy(x => x.Name)
            .Select(group => new LocalizationResourceProjection
            {
                Name = group.Key,
                DefaultCulture = group.FirstOrDefault(x => x.IsDefault)?.Culture ?? group.First().Culture,
                Cultures = [.. group.Select(x => x.Culture)]
            }).ToImmutableArray();
    }
}