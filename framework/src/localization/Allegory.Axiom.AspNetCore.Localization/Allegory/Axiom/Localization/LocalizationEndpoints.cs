using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Localization;

public static class LocalizationEndpoints
{
    private static ImmutableList<ApplicationLanguageDto>? _languages;
    public static ImmutableList<ApplicationLanguageDto> Languages => _languages!;

    public static void MapLocalizationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/axiom/application/localization");

        group.MapGet("/languages", GetLanguages);
        group.MapGet("/translations", GetTranslations);
    }

    private static IResult GetLanguages(IOptions<RequestLocalizationOptions> options)
    {
        if (_languages is null)
        {
            var cultures = options.Value.SupportedUICultures ?? options.Value.SupportedCultures;
            _languages = cultures?.Select(x => new ApplicationLanguageDto
            {
                Name = x.Name,
                NativeName = x.NativeName
            }).ToImmutableList() ?? ImmutableList<ApplicationLanguageDto>.Empty;
        }

        return Results.Ok(Languages);
    }

    private static IResult GetTranslations(ILocalizationProvider provider, IStringLocalizerFactory factory)
    {
        var translations = new Dictionary<string, Dictionary<string, string>>();

        foreach (var context in provider.Contexts)
        {
            var localizer = factory.Create(context.Key, string.Empty);
            translations[context.Key] = localizer.GetAllStrings(true).ToDictionary(x => x.Name, x => x.Value);
        }

        return Results.Ok(translations);
    }
}