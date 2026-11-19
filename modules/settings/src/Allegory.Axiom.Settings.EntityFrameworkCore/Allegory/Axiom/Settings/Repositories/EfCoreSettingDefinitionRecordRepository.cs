using System;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Settings;

public class EfCoreSettingDefinitionRecordRepository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, SettingDefinitionRecord, Guid>(serviceProvider),
    ISettingDefinitionRecordRepository
    where TDbContext : DbContext;