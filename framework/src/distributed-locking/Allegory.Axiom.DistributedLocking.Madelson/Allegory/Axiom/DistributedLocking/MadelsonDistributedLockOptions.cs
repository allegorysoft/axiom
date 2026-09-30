using Allegory.Axiom.Redis;

namespace Allegory.Axiom.DistributedLocking;

public class MadelsonDistributedLockOptions
{
    public string ConnectionName { get; set; } = RedisOptions.DefaultConnectionName;
}