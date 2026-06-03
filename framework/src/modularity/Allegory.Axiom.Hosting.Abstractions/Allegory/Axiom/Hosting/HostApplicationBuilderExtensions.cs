using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allegory.Axiom.Hosting;

public static class HostApplicationBuilderExtensions
{
    internal static readonly ConditionalWeakTable<IHostApplicationBuilder, ExtraProperties> BuilderProperties = new();

    extension(IHostApplicationBuilder builder)
    {
        public async Task<AxiomApplication> ConfigureApplicationAsync(
            Action<AxiomApplicationOptions>? optionsAction = null)
        {
            var options = new AxiomApplicationOptions();
            optionsAction?.Invoke(options);

            options.StartupAssembly ??= Assembly.GetEntryAssembly();
            ArgumentNullException.ThrowIfNull(options.StartupAssembly);

            options.ApplicationBuilder ??= new AxiomApplicationBuilder();

            var application = await options.ApplicationBuilder.BuildAsync(
                new AxiomApplicationBuilderContext(
                    builder,
                    options.StartupAssembly,
                    options.DependencyRegistrar ??= new AssemblyDependencyRegistrar(builder.Services),
                    options.Plugins));

            builder.ExecuteDeferredActions();
            builder.ExecuteBuilderActions();

            return application;
        }

        public AxiomApplication GetAxiomApplication()
        {
            var application = builder.Services.Single(service => service.ServiceType == typeof(AxiomApplication))
                              ?? throw new InvalidOperationException("AxiomApplication is not registered.");

            return (AxiomApplication) application.ImplementationInstance!;
        }

        public void AddDeferredAction(Action<IHostApplicationBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            BuilderProperties.GetOrCreateValue(builder).DeferredActions.Add(action);
        }

        private void ExecuteDeferredActions()
        {
            var extraProperties = BuilderProperties.GetOrCreateValue(builder);

            foreach (var action in extraProperties.DeferredActions)
            {
                action(builder);
            }

            extraProperties.DeferredActions.Clear();
        }

        public void AddBuilder<T>(T builderInstance)
        {
            ArgumentNullException.ThrowIfNull(builderInstance);
            var contexts = BuilderProperties.GetOrCreateValue(builder).BuilderContexts;

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
            var contexts = BuilderProperties.GetOrCreateValue(builder).BuilderContexts;

            if (!contexts.TryGetValue(typeof(T), out var ctx))
            {
                contexts[typeof(T)] = ctx = new BuilderContext<T>();
            }

            ((BuilderContext<T>) ctx).Actions.Add(action);
        }

        private void ExecuteBuilderActions()
        {
            var contexts = BuilderProperties.GetOrCreateValue(builder).BuilderContexts;

            foreach (var context in contexts.Values)
            {
                context.Execute();
            }

            contexts.Clear();
        }
    }

    internal class ExtraProperties
    {
        public List<Action<IHostApplicationBuilder>> DeferredActions { get; } = [];
        public Dictionary<Type, IBuilderContext> BuilderContexts { get; } = [];
    }
}