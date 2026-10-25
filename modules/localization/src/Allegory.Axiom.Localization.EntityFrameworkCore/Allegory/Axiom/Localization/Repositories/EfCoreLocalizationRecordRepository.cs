using System;
using Allegory.Axiom.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Allegory.Axiom.Localization;

public class EfCoreLocalizationRecordRepository<TDbContext>(
    IServiceProvider serviceProvider) :
    EfCoreRepository<TDbContext, LocalizationRecord, Guid>(serviceProvider),
    ILocalizationRecordRepository
    where TDbContext : DbContext { }