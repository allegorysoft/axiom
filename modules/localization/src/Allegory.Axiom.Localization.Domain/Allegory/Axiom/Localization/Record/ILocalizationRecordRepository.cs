using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;

namespace Allegory.Axiom.Localization;

public interface ILocalizationRecordRepository : IRepository<LocalizationRecord, Guid>
{
    Task<ImmutableArray<LocalizationResourceProjection>> GetResourceListAsync(
        CancellationToken cancellationToken = default);
}