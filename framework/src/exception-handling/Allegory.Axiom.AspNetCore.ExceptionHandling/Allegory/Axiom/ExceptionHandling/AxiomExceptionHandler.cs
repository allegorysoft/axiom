using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LocalizationOptions=Allegory.Axiom.Localization.LocalizationOptions;

namespace Allegory.Axiom.ExceptionHandling;

public class AxiomExceptionHandler(
    ILogger<AxiomExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IStringLocalizerFactory localizerFactory,
    IOptions<LocalizationOptions> localizationOptions)
    : IExceptionHandler
{
    protected ILogger<AxiomExceptionHandler> Logger { get; } = logger;
    protected IProblemDetailsService ProblemDetailsService { get; } = problemDetailsService;
    protected IStringLocalizerFactory LocalizerFactory { get; } = localizerFactory;
    protected LocalizationOptions LocalizationOptions { get; } = localizationOptions.Value;

    public virtual async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception ex,
        CancellationToken cancellationToken)
    {
        if (ex is not AxiomException exception)
        {
            return false;
        }

        LoggerExtensions.LogException(
            Logger,
            exception.LogLevel,
            exception,
            exception.Code,
            exception.HttpStatusCode);

        context.Response.StatusCode = (int) exception.HttpStatusCode;

        var problem = new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = GetProblemDetails(exception)
        };

        return await ProblemDetailsService.TryWriteAsync(problem);
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

            //optimize here
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

        var index = exception.Code.IndexOf(':');
        if (index == -1)
        {
            return null;
        }

        var exceptionCodePrefix = exception.Code[..index];

        if (!LocalizationOptions.ExceptionCodeMappings.TryGetValue(
                exceptionCodePrefix, out var localizationResource))
        {
            return null;
        }

        var localizer = LocalizerFactory.Create(localizationResource, string.Empty);
        return localizer[exception.Code];
    }
}