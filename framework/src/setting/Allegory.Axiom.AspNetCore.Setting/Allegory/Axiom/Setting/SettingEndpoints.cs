using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Allegory.Axiom.Setting;

public static class SettingEndpoints
{
    public static void MapSettingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/axiom/application/settings")
            .WithTags("application", "settings");

        // AddApplicationContributor("settings", GetSettings)

        group.MapGet("/", GetSettingsAsync);
    }

    private static async ValueTask<IResult> GetSettingsAsync(ISettingDefinitionStore store)
    {
        var values = new Dictionary<string, object>();

        var settings = await store.GetListAsync();
        foreach (var setting in settings.Where(s => s.IsPublic))
        {
            if (setting.DefaultValue.HasValue)
            {
                values[setting.Name] = setting.DefaultValue;
            }
        }

        return Results.Ok(values);
    }
}