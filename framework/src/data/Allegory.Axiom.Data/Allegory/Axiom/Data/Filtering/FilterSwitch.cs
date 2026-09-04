using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.Disposables;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Data.Filtering;

public class FilterSwitch(IOptions<FilterSwitchOptions> options) : IFilterSwitch, ISingletonService
{
    protected ConcurrentDictionary<Type, AsyncLocal<bool?>> Filters { get; } = new();
    protected FrozenDictionary<Type, bool> Defaults { get; } = options.Value.Defaults.ToFrozenDictionary();

    public virtual bool IsEnabled<T>()
    {
        var filter = GetFilter<T>();
        return filter.Value ?? Defaults.GetValueOrDefault(typeof(T), true);
    }

    public virtual IDisposable Enable<T>()
    {
        var filter = GetFilter<T>();
        var previous = filter.Value;

        if (previous == true)
        {
            return EmptyDisposable.Instance;
        }

        filter.Value = true;

        return new DisposableDelegate<(AsyncLocal<bool?>, bool?)>(
            static state =>
            {
                var (f, p) = state;
                f.Value = p;
            }, (filter, previous));
    }

    public virtual IDisposable Disable<T>()
    {
        var filter = GetFilter<T>();
        var previous = filter.Value;

        if (previous == false)
        {
            return EmptyDisposable.Instance;
        }

        filter.Value = false;

        return new DisposableDelegate<(AsyncLocal<bool?>, bool?)>(
            static state =>
            {
                var (f, p) = state;
                f.Value = p;
            }, (filter, previous));
    }

    protected virtual AsyncLocal<bool?> GetFilter<T>()
    {
        return Filters.GetOrAdd(typeof(T), static _ => new AsyncLocal<bool?>());
    }
}