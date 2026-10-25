using System;
using Allegory.Axiom.Domain.Repositories;

namespace Allegory.Axiom.Localization;

public interface ILocalizationRecordRepository : IRepository<LocalizationRecord, Guid> { }