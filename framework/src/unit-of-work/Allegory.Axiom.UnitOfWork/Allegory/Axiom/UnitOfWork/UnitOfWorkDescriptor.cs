using System.Reflection;

namespace Allegory.Axiom.UnitOfWork;

public readonly struct UnitOfWorkDescriptor(bool isEnabled, UnitOfWorkOptions? optionses = null)
{
    public bool IsEnabled { get; } = isEnabled;
    public UnitOfWorkOptions? Options { get; } = optionses;
}