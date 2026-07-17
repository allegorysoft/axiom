using System.Diagnostics;

namespace Allegory.Axiom.UnitOfWork;

public static class UnitOfWorkActivity
{
    public const string Name = "Allegory.Axiom.UnitOfWork";
    public static readonly ActivitySource Source = new(Name);
}