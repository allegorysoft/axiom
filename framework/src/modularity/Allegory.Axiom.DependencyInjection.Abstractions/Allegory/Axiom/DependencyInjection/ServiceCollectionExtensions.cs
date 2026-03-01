using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static readonly ConditionalWeakTable<
        IServiceCollection,
        List<Action<IServiceCollection>>> PostConfigureActions = new();

    extension(IServiceCollection collection)
    {
        public IReadOnlyList<Action<IServiceCollection>> PostConfigureActions =>
            PostConfigureActions.GetOrCreateValue(collection);

        public void AddPostConfigureAction(Action<IServiceCollection> action)
        {
            PostConfigureActions.GetOrCreateValue(collection).Add(action);
        }
    }
}