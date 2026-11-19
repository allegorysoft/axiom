using System;
using Allegory.Axiom.Domain.Repositories;

namespace Allegory.Axiom.Settings;

public interface ISettingDefinitionRecordRepository : IRepository<SettingDefinitionRecord, Guid> { }