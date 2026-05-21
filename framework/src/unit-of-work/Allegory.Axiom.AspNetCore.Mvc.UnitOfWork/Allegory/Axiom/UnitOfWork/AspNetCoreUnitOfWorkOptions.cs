using System;
using Microsoft.AspNetCore.Http;

namespace Allegory.Axiom.UnitOfWork;

public class AspNetCoreUnitOfWorkOptions
{
    public Func<HttpContext, UnitOfWorkOptions?>? OptionsSelector { get; set; }
}