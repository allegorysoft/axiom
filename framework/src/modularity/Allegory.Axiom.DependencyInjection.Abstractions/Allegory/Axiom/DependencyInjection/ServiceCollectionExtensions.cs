using System;
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
    }
}