using System;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Setting;

public class EfCoreSettingValueRecordRepository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, SettingValueRecord, Guid>(serviceProvider),
    ISettingValueRecordRepository
    where TDbContext : DbContext
{
}