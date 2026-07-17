using System.Diagnostics;

namespace Allegory.Axiom.EventBus;

public static class EventBusActivity
{
    public const string Name = "Allegory.Axiom.EventBus";
    public static readonly ActivitySource Source = new(Name);
}