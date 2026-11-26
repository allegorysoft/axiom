using System;
using Allegory.Axiom.Domain.Repositories;

namespace Allegory.Axiom.Setting;

public interface ISettingValueRecordRepository : IRepository<SettingValueRecord, Guid>
{
}