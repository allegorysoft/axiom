using System;
using System.Collections.Generic;

namespace Allegory.Axiom.Hosting;

internal class BuilderContext<T> : IBuilderContext
{
    public T Builder { get; set; } = default!;
    public List<Action<T>> Actions { get; } = [];

    public void Execute()
    {
        ArgumentNullException.ThrowIfNull(Builder);

        foreach (var action in Actions)
        {
            action(Builder);
        }
    }
}

internal interface IBuilderContext
{
    void Execute();
}