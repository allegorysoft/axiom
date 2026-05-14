using System;
using System.Collections.Generic;
using System.Net;
using Allegory.Axiom.Exceptions;
using Microsoft.Extensions.Logging;

namespace Allegory.Axiom.AspNetCore.ExceptionHandling;

public class AspNetCoreExceptionHandlerOptions
{
    public Dictionary<Type, LogLevel> ExceptionLogLevels { get; } = [];
    public Dictionary<Type, HttpStatusCode> ExceptionStatusCodes { get; } = [];

    public void AddLogLevel<TException>(LogLevel logLevel) where TException : AxiomException
    {
        ExceptionLogLevels.Add(typeof(TException), logLevel);
    }

    public void AddStatusCode<TException>(HttpStatusCode statusCode) where TException : AxiomException
    {
        ExceptionStatusCodes.Add(typeof(TException), statusCode);
    }
}