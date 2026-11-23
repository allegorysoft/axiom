using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Setting;

public class EfCoreSettingDefinitionRecordRepository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, SettingDefinitionRecord, Guid>(serviceProvider),
    ISettingDefinitionRecordRepository
    where TDbContext : DbContext
{
    public virtual async Task<ImmutableArray<SettingDefinition>> GetDefinitionListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken = GetCancellationToken(cancellationToken);
        var queryable = await GetQueryableAsync(false, cancellationToken);

        var records = await queryable
            .Select(x => new
            {
                x.Name,
                x.DefaultValue,
                x.IsPublic,
                x.IsEncrypted,
                x.Contributors
            })
            .ToListAsync(cancellationToken);

        return records
            .Select(x => new SettingDefinition(
                x.Name,
                x.DefaultValue == null ? null : JsonElement.Parse(x.DefaultValue),
                x.IsPublic,
                x.IsEncrypted,
                x.Contributors))
            .ToImmutableArray();
    }
}