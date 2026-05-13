using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Allegory.Axiom.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;
using LocalizationOptions=Allegory.Axiom.Localization.LocalizationOptions;

namespace Allegory.Axiom.AspNetCore.ExceptionHandling;

public class AxiomExceptionHandlerTests
    : IClassFixture<IntegrationTestFixture>
{
    public AxiomExceptionHandlerTests(IntegrationTestFixture fixture)
    {
        Fixture = fixture;
        Logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        ProblemDetailsService.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(true);
    }

    protected IntegrationTestFixture Fixture { get; }
    protected ILogger<AxiomExceptionHandler> Logger { get; } = Substitute.For<ILogger<AxiomExceptionHandler>>();
    protected MockProblemDetailsService ProblemDetailsService { get; } = Substitute.For<MockProblemDetailsService>();
    protected IStringLocalizerFactory LocalizerFactory { get; } = Substitute.For<IStringLocalizerFactory>();

    protected virtual AxiomExceptionHandler CreateHandler(
        AspNetCoreExceptionHandlerOptions? handlerOptions = null,
        LocalizationOptions? localizationOptions = null)
    {
        return new AxiomExceptionHandler(
            Logger,
            ProblemDetailsService,
            LocalizerFactory,
            Options.Create(handlerOptions ?? Fixture.Service<IOptions<AspNetCoreExceptionHandlerOptions>>().Value),
            Options.Create(localizationOptions ?? new LocalizationOptions()));
    }

    [Fact]
    public async Task ShouldReturnFalseForNonAxiomException()
    {
        var handler = CreateHandler();
        var result = await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new InvalidOperationException("boom"),
            CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldReturnTrueForAxiomException()
    {
        var handler = CreateHandler();
        var result = await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(),
            CancellationToken.None);

        result.ShouldBeTrue();
    }

    // Logging

    [Fact]
    public async Task ShouldLogForMappedException()
    {
        var handler = CreateHandler();

        await handler.TryHandleAsync(new DefaultHttpContext(), new AuthorizationException(), CancellationToken.None);

        Logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<AuthorizationException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ShouldLogFromBaseTypeWhenDerivedNotMapped()
    {
        var handler = CreateHandler();

        await handler.TryHandleAsync(
            new DefaultHttpContext(), new DerivedAuthorizationException(), CancellationToken.None);

        Logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<AuthorizationException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ShouldNotLogWhenExceptionNotMapped()
    {
        var handler = CreateHandler();

        await handler.TryHandleAsync(new DefaultHttpContext(), new UnmappedAxiomException(), CancellationToken.None);

        Logger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    // Status code resolution

    [Fact]
    public async Task ShouldSetStatusCodeForMappedException()
    {
        var handler = CreateHandler();
        var context = new DefaultHttpContext();

        await handler.TryHandleAsync(context, new NotFoundException(), CancellationToken.None);

        context.Response.StatusCode.ShouldBe((int) HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldSetStatusCodeFromBaseTypeWhenDerivedNotMapped()
    {
        var handler = CreateHandler();
        var context = new DefaultHttpContext();

        await handler.TryHandleAsync(context, new DerivedNotFoundException(), CancellationToken.None);

        context.Response.StatusCode.ShouldBe((int) HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotSetStatusCodeWhenExceptionNotMapped()
    {
        var handler = CreateHandler();
        var context = new DefaultHttpContext();
        var originalStatusCode = context.Response.StatusCode;

        await handler.TryHandleAsync(context, new UnmappedAxiomException(), CancellationToken.None);

        context.Response.StatusCode.ShouldBe(originalStatusCode);
    }

    // Problem details — title and detail

    [Fact]
    public async Task ShouldSetTitleToExceptionCode()
    {
        ProblemDetails? captured = null;
        ProblemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c.ProblemDetails))
            .Returns(true);

        var handler = CreateHandler();
        await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(code: "BIZ:001", message: "Something went wrong"),
            CancellationToken.None);

        captured.ShouldNotBeNull();
        captured.Title.ShouldBe("BIZ:001");
    }

    [Fact]
    public async Task ShouldSetDetailToExceptionMessage()
    {
        ProblemDetails? captured = null;
        ProblemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c.ProblemDetails))
            .Returns(true);

        var handler = CreateHandler();
        await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(code: "BIZ:001", message: "Something went wrong"),
            CancellationToken.None);

        captured!.Detail.ShouldBe("Something went wrong");
    }

    [Fact]
    public async Task ShouldInterpolateExceptionDataIntoDetail()
    {
        ProblemDetails? captured = null;
        ProblemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c.ProblemDetails))
            .Returns(true);

        var exception = new BusinessException(code: "BIZ:001", message: "Entity {id} not found");
        exception.Data["id"] = 42;

        var handler = CreateHandler();
        await handler.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);

        captured!.Detail.ShouldBe("Entity 42 not found");
    }

    [Fact]
    public async Task ShouldAddExceptionDataToExtensions()
    {
        ProblemDetails? captured = null;
        ProblemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c.ProblemDetails))
            .Returns(true);

        var exception = new BusinessException(code: "BIZ:001", message: "Entity {id} not found");
        exception.Data["id"] = 42;

        var handler = CreateHandler();
        await handler.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None);

        captured!.Extensions.ShouldContainKeyAndValue("id", 42);
    }

    // Localization

    [Fact]
    public async Task ShouldLocalizeDetailWhenMessageIsEmptyAndCodeMatchesMapping()
    {
        ProblemDetails? captured = null;
        ProblemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c.ProblemDetails))
            .Returns(true);

        const string code = "BIZ:001";
        const string localized = "Localized message";

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[code].Returns(new LocalizedString(code, localized));
        LocalizerFactory
            .Create("MyApp.Resources.Messages", string.Empty)
            .Returns(localizer);

        var localizationOptions = new LocalizationOptions();
        localizationOptions.ExceptionCodeMappings["BIZ"] = "MyApp.Resources.Messages";

        var handler = CreateHandler(localizationOptions: localizationOptions);
        await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(code: code),// no message → triggers localization
            CancellationToken.None);

        captured!.Detail.ShouldBe(localized);
    }

    [Fact]
    public async Task ShouldNotLocalizeWhenMessageIsPresent()
    {
        ProblemDetails? captured = null;
        ProblemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c.ProblemDetails))
            .Returns(true);

        var localizationOptions = new LocalizationOptions();
        localizationOptions.ExceptionCodeMappings["BIZ"] = "MyApp.Resources.Messages";

        var handler = CreateHandler(localizationOptions: localizationOptions);
        await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(code: "BIZ:001", message: "Already has message"),
            CancellationToken.None);

        LocalizerFactory.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<string>());
        captured!.Detail.ShouldBe("Already has message");
    }

    [Fact]
    public async Task ShouldNotLocalizeWhenCodeHasNoPrefix()
    {
        var handler = CreateHandler();
        await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(code: "NoPrefixCode"),
            CancellationToken.None);

        LocalizerFactory.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ShouldNotLocalizeWhenPrefixNotInMappings()
    {
        var localizationOptions = new LocalizationOptions();
        localizationOptions.ExceptionCodeMappings["OTHER"] = "MyApp.Resources.Other";

        var handler = CreateHandler(localizationOptions: localizationOptions);
        await handler.TryHandleAsync(
            new DefaultHttpContext(),
            new BusinessException(code: "BIZ:001"),
            CancellationToken.None);

        LocalizerFactory.DidNotReceive().Create(Arg.Any<string>(), Arg.Any<string>());
    }

    // Helpers

    private class DerivedNotFoundException(string? code = null, string? message = null)
        : NotFoundException(code, message);

    private class DerivedAuthorizationException(string? code = null, string? message = null)
        : AuthorizationException(code, message);

    private class UnmappedAxiomException(string? code = null, string? message = null)
        : AxiomException(code, message);
}

public class MockProblemDetailsService : IProblemDetailsService
{
    public virtual ValueTask WriteAsync(ProblemDetailsContext context)
    {
        return ValueTask.CompletedTask;
    }

    public virtual ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
    {
        return ValueTask.FromResult(true);
    }
}