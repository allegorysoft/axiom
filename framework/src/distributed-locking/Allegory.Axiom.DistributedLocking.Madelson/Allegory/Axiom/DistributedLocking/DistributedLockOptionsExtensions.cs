using Allegory.Axiom.Extensibility;

namespace Allegory.Axiom.DistributedLocking;

public static class DistributedLockOptionsExtensions
{
    extension(DistributedLockOptions options)
    {
        public MadelsonDistributedLockOptions Madelson
        {
            get => options.GetOrAddProperty(
                DistributedLockingMadelsonPackage.Section,
                static () => new MadelsonDistributedLockOptions());

            set => options.SetProperty(DistributedLockingMadelsonPackage.Section, value);
        }
    }
}