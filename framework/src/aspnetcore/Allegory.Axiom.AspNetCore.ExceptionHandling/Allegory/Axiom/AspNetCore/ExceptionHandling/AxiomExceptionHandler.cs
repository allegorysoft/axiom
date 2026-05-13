using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LocalizationOptions=Allegory.Axiom.Localization.LocalizationOptions;

namespace Allegory.Axiom.AspNetCore.ExceptionHandling;

public class AxiomExceptionHandler(
    ILogger<AxiomExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IStringLocalizerFactory localizerFactory,
    IOptions<AspNetCoreExceptionHandlerOptions> options,
    IOptions<LocalizationOptions> localizationOptions)
    : IExceptionHandler
{
    protected ILogger<AxiomExceptionHandler> Logger { get; } = logger;
    protected IProblemDetailsService ProblemDetailsService { get; } = problemDetailsService;
    protected IStringLocalizerFactory LocalizerFactory { get; } = localizerFactory;
    protected AspNetCoreExceptionHandlerOptions Options { get; } = options.Value;
    protected Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> ExceptionCodeLookup { get; }
        = localizationOptions.Value.ExceptionCodeMappings.GetAlternateLookup<ReadOnlySpan<char>>();

    public virtual async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception ex,
        CancellationToken cancellationToken)
    {
        if (ex is not AxiomException exception)
        {
            return false;
        }

        TryLogException(exception);
        TrySetStatusCode(context, exception);

        var problem = new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = GetProblemDetails(exception)
        };

        return await ProblemDetailsService.TryWriteAsync(problem);
    }

    protected virtual void TryLogException(AxiomException exception)
    {
        var type = exception.GetType();

        while (type != typeof(AxiomException) && type != null)
        {
            if (Options.ExceptionLogLevels.TryGetValue(type, out var logLevel))
            {
                LoggerExtensions.LogException(
                    Logger,
                    logLevel,
                    exception,
                    exception.Code);
                break;
            }

            type = type.BaseType;
        }
    }

    protected virtual void TrySetStatusCode(HttpContext context, AxiomException exception)
    {
        var type = exception.GetType();

        while (type != typeof(AxiomException) && type != null)
        {
            if (Options.ExceptionStatusCodes.TryGetValue(type, out var statusCode))
            {
                context.Response.StatusCode = (int) statusCode;
                break;
            }

            type = type.BaseType;
        }
    }

    protected virtual ProblemDetails GetProblemDetails(AxiomException exception)
    {
        var problem = new ProblemDetails
        {
            Title = exception.Code,
            Detail = TryLocalizeDetail(exception) ?? exception.Message,
        };

        foreach (DictionaryEntry data in exception.Data)
        {
            if (data.Key is not string key)
            {
                continue;
            }

            problem.Extensions[key] = data.Value;

            //Optimize here
            problem.Detail = problem.Detail.Replace(
                "{" + key + "}",
                data.Value?.ToString());
        }

        return problem;
    }

    protected virtual string? TryLocalizeDetail(AxiomException exception)
    {
        if (!string.IsNullOrWhiteSpace(exception.Message) || string.IsNullOrWhiteSpace(exception.Code))
        {
            return null;
        }

        ReadOnlySpan<char> code = exception.Code;

        var index = code.IndexOf(':');
        if (index <= 0)
        {
            return null;
        }

        if (!ExceptionCodeLookup.TryGetValue(code[..index], out var localizationResource))
        {
            return null;
        }

        var localizer = LocalizerFactory.Create(localizationResource, string.Empty);
        return localizer[exception.Code];
    }
}