using System;

namespace Allegory.Axiom.Data.Filtering;

public interface IFilterSwitch
{
    bool IsEnabled<T>();
    IDisposable Enable<T>();
    IDisposable Disable<T>();
}