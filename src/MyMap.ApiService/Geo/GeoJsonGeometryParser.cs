using System.Text.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace MyMap.ApiService.Geo;

public static class GeoJsonGeometryParser
{
    // Parses a GeoJSON Polygon or MultiPolygon into an NTS Geometry with SRID=4326.
    // Returns null on invalid input.
    public static Geometry? ParsePolygonOrMultiPolygon(JsonElement elem)
    {
        if (elem.ValueKind != JsonValueKind.Object) return null;

        if (!elem.TryGetProperty("type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String) return null;
        var type = typeProp.GetString();

        if (!elem.TryGetProperty("coordinates", out var coords)) return null;

        var gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        return type switch
        {
            "Polygon" => BuildPolygon(coords, gf),
            "MultiPolygon" => BuildMultiPolygon(coords, gf),
            _ => null
        };
    }

    private static Polygon? BuildPolygon(JsonElement coords, GeometryFactory gf)
    {
        if (coords.ValueKind != JsonValueKind.Array || coords.GetArrayLength() == 0) return null;

        var shell = BuildLinearRing(coords[0], gf);
        if (shell is null) return null;

        var holes = new List<LinearRing>();
        for (int i = 1; i < coords.GetArrayLength(); i++)
        {
            var ring = BuildLinearRing(coords[i], gf);
            if (ring is null) return null;
            holes.Add(ring);
        }

        return gf.CreatePolygon(shell, holes.Count == 0 ? null : holes.ToArray());
    }

    private static MultiPolygon? BuildMultiPolygon(JsonElement coords, GeometryFactory gf)
    {
        if (coords.ValueKind != JsonValueKind.Array || coords.GetArrayLength() == 0) return null;
        var polys = new List<Polygon>();
        foreach (var polyCoords in coords.EnumerateArray())
        {
            var poly = BuildPolygon(polyCoords, gf);
            if (poly is null) return null;
            polys.Add(poly);
        }
        return gf.CreateMultiPolygon(polys.ToArray());
    }

    private static LinearRing? BuildLinearRing(JsonElement lineStringCoords, GeometryFactory gf)
    {
        if (lineStringCoords.ValueKind != JsonValueKind.Array || lineStringCoords.GetArrayLength() < 4) return null;
        var points = new List<Coordinate>(lineStringCoords.GetArrayLength());
        foreach (var pos in lineStringCoords.EnumerateArray())
        {
            if (pos.ValueKind != JsonValueKind.Array || pos.GetArrayLength() < 2) return null;
            var lon = pos[0].GetDouble();
            var lat = pos[1].GetDouble();
            points.Add(new Coordinate(lon, lat));
        }
        // Ensure closed ring
        if (!points[0].Equals2D(points[^1]))
        {
            points.Add(points[0]);
        }
        return gf.CreateLinearRing(points.ToArray());
    }
}
