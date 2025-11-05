using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyMap.ApiService.Data;
using MyMap.ApiService.Types;
using MyMap.ApiService.Geo;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace MyMap.ApiService.Endpoints;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder api)
    {
        // GET /api/features?bbox=west,south,east,north&category=&limit=
        api.MapGet(
                "/features",
                async (HttpContext httpContext,
                       AppDbContext db,
                       string? bbox,
                       string? category,
                       int? limit,
                       CancellationToken ct) =>
                {
                    if (string.IsNullOrWhiteSpace(bbox))
                    {
                        return Results.Ok(Array.Empty<FeatureDto>());
                    }

                    var parts = bbox.Split(',', StringSplitOptions.TrimEntries);
                    if (parts.Length != 4 ||
                        !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var west) ||
                        !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var south) ||
                        !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var east) ||
                        !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var north))
                    {
                        return Results.BadRequest("Invalid bbox. Expected west,south,east,north as doubles.");
                    }

                    var max = 2000;
                    var take = Math.Min(Math.Max(limit ?? 500, 1), max);

                    var active = httpContext.Items["ActiveDataVersion"] as string ?? string.Empty;

                    // Build an SRID=4326 envelope polygon
                    var gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                    var env = new Envelope(west, east, south, north);
                    var bboxGeom = gf.ToGeometry(env);

                    var query = db.Pois
                        .AsNoTracking()
                        .Where(p => p.DataVersion == active)
                        .Where(p => bboxGeom.Intersects(p.Geom));

                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        query = query.Where(p => p.Category == category);
                    }

                    var list = await query
                        .OrderByDescending(p => p.UpdatedAtUtc)
                        .Take(take)
                        .Select(p => new FeatureDto(
                            p.Id,
                            p.Name,
                            p.Category,
                            p.Geom.Y,
                            p.Geom.X,
                            p.UpdatedAtUtc))
                        .ToListAsync(ct);

                    return Results.Ok(list);
                }
            );

        // POST /api/features/query - body is GeoJSON Polygon or MultiPolygon
        api.MapPost(
                "/features/query",
                async (HttpContext httpContext,
                       AppDbContext db,
                       JsonElement? polygon,
                       string? category,
                       int? limit,
                       CancellationToken ct) =>
                {
                    if (polygon is null || polygon.Value.ValueKind == JsonValueKind.Undefined || polygon.Value.ValueKind == JsonValueKind.Null)
                    {
                        return Results.BadRequest("Missing polygon body.");
                    }

                    var region = GeoJsonGeometryParser.ParsePolygonOrMultiPolygon(polygon.Value);
                    if (region is null)
                    {
                        return Results.BadRequest("Invalid GeoJSON Polygon/MultiPolygon.");
                    }

                    var max = 2000;
                    var take = Math.Min(Math.Max(limit ?? 500, 1), max);
                    var active = httpContext.Items["ActiveDataVersion"] as string ?? string.Empty;

                    var query = db.Pois
                        .AsNoTracking()
                        .Where(p => p.DataVersion == active)
                        .Where(p => p.Geom.Within(region));

                    if (!string.IsNullOrWhiteSpace(category))
                    {
                        query = query.Where(p => p.Category == category);
                    }

                    var list = await query
                        .OrderByDescending(p => p.UpdatedAtUtc)
                        .Take(take)
                        .Select(p => new FeatureDto(
                            p.Id,
                            p.Name,
                            p.Category,
                            p.Geom.Y,
                            p.Geom.X,
                            p.UpdatedAtUtc))
                        .ToListAsync(ct);

                    return Results.Ok(list);
                }
            );

        return api;
    }

}
