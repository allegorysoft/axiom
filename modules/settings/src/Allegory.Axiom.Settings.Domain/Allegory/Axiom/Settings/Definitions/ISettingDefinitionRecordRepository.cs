using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Domain.Repositories;

namespace Allegory.Axiom.Settings;

public interface ISettingDefinitionRecordRepository : IRepository<SettingDefinitionRecord, Guid>
{
    Task<ImmutableArray<SettingDefinition>> GetDefinitionListAsync(
        CancellationToken cancellationToken = default);
}