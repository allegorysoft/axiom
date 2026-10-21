using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.Localization;

public static class LocalizationEndpoints
{
    public static RouteGroupBuilder MapLocalizationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/axiom/application/localization");

        group.MapGet("/languages", GetLanguages);

        return group;
    }

    private static IResult GetLanguages(IOptions<RequestLocalizationOptions> options)
    {
        var cultures = options.Value.SupportedUICultures ?? options.Value.SupportedCultures;

        return Results.Ok(cultures?.Select(x => new
        {
            x.Name,
            x.NativeName
        }).ToArray() ?? []);
    }
}