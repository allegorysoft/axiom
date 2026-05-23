using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Allegory.Axiom.DependencyInjection;

public static class ServiceCollectionExtensions
{
    internal static readonly ConditionalWeakTable<
        IServiceCollection,
        ServiceCollectionExtraProperties> ExtraProperties = new();

    extension(IServiceCollection collection)
    {
        public void AddPostConfigureAction(Action<IServiceCollection> action)
        {
            ExtraProperties.GetOrCreateValue(collection)
                .PostConfigureActions
                .Add(action);
        }

        public void ExecutePostConfigureActions()
        {
            var extraProperties = ExtraProperties.GetOrCreateValue(collection);

            foreach (var action in extraProperties.PostConfigureActions)
            {
                action(collection);
            }
        }

        public void AddBuilderAction<T>(Action<T> configure) where T : class
        {
            var store = ExtraProperties.GetOrCreateValue(collection).BuilderActions;

            if (!store.TryGetValue(typeof(T), out var actions))
            {
                actions = new List<Action<T>>();
                store[typeof(T)] = actions;
            }

            ((List<Action<T>>) actions).Add(configure);
        }

        public void ExecuteBuilderActions<T>(T options)
        {
            var store = ExtraProperties.GetOrCreateValue(collection).BuilderActions;

            if (!store.TryGetValue(typeof(T), out var actions))
            {
                return;
            }

            foreach (var action in (List<Action<T>>) actions)
            {
                action(options);
            }
        }
    }
}