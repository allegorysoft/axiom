using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public static class HostExtensions
{
    internal static readonly ConditionalWeakTable<IHost, ExtraProperties> HostProperties = new();

    extension(IHost host)
    {
        public async Task InitializeApplicationAsync()
        {
            //TODO: Add concurrent parameter

            var application = host.Services.GetRequiredService<AxiomApplication>();

            foreach (var assembly in application.Assemblies)
            {
                var configureMethod = assembly.GetTypes()
                    .SingleOrDefault(t => typeof(IInitializeApplication).IsAssignableFrom(t)
                                          && t is {IsClass: true, IsAbstract: false})?
                    .GetMethod(nameof(IInitializeApplication.InitializeAsync));

                if (configureMethod != null)
                {
                    await (Task) configureMethod.Invoke(null, [host])!;
                }
            }

            host.ExecuteBuilderActions();
        }

        public void AddBuilder<T>(T builderInstance)
        {
            ArgumentNullException.ThrowIfNull(builderInstance);
            var contexts = HostProperties.GetOrCreateValue(host).BuilderContexts;

            if (!contexts.TryGetValue(typeof(T), out var ctx))
            {
                contexts[typeof(T)] = new BuilderContext<T> {Builder = builderInstance};
                return;
            }

            var context = (BuilderContext<T>) ctx;
            if (context.Builder != null)
            {
                throw new InvalidOperationException(
                    $"A builder of type '{typeof(T).FullName}' is already registered.");
            }

            context.Builder = builderInstance;
        }

        public void AddBuilderAction<T>(Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            var contexts = HostProperties.GetOrCreateValue(host).BuilderContexts;

            if (!contexts.TryGetValue(typeof(T), out var ctx))
            {
                contexts[typeof(T)] = ctx = new BuilderContext<T>();
            }

            ((BuilderContext<T>) ctx).Actions.Add(action);
        }

        private void ExecuteBuilderActions()
        {
            var contexts = HostProperties.GetOrCreateValue(host).BuilderContexts;

            foreach (var context in contexts.Values)
            {
                context.Execute();
            }

            contexts.Clear();
        }
    }

    internal class ExtraProperties
    {
        public Dictionary<Type, IBuilderContext> BuilderContexts { get; } = [];
    }
}