using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        group.MapGet("/translations", GetTranslationsAsync);
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

    private static async ValueTask<IResult> GetTranslationsAsync()
    {
        return Results.Ok();
    }
}