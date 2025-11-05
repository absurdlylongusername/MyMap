using Microsoft.EntityFrameworkCore;
using MyMap.ApiService.Data;
using MyMap.ApiService.Types;

namespace MyMap.ApiService.Endpoints;

public static class DatasetEndpoints
{
    public static IEndpointRouteBuilder MapDatasetEndpoints(this IEndpointRouteBuilder api)
    {
        // GET /api/datasets/version
        api.MapGet(
                "/datasets/version",
                async (AppDbContext db, CancellationToken ct) =>
                {
                    var active = await db.DatasetMeta
                        .AsNoTracking()
                        .Where(m => m.Id == 1)
                        .Select(m => m.ActiveVersion)
                        .FirstOrDefaultAsync(ct) ?? string.Empty;

                    return Results.Ok(new { activeVersion = active });
                }
            );

        // GET /api/datasets/version/{version}
        api.MapGet(
                "/datasets/version/{version}",
                async (string version, AppDbContext db, CancellationToken ct) =>
                {
                    var dto = await db.DatasetVersions
                        .AsNoTracking()
                        .Where(v => v.Version == version)
                        .Select(v => new DatasetVersionDto(
                            v.Version,
                            v.Source,
                            v.PulledAtUtc,
                            v.TransformsJson))
                        .FirstOrDefaultAsync(ct);

                    return dto is null ? Results.NotFound() : Results.Ok(dto);
                }
            );

        return api;
    }
}
