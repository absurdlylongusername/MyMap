using System.Text.Json;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using MyMap.ApiService.Geo;

namespace MyMap.ApiService.Tests;

public class GeoJsonGeometryParserTests
{
    [Test]
    public void Parse_Valid_Polygon_Works()
    {
        var json = """
        {
          "type": "Polygon",
          "coordinates": [
            [
              [-122.55, 37.70],
              [-122.35, 37.70],
              [-122.35, 37.84],
              [-122.55, 37.84],
              [-122.55, 37.70]
            ]
          ]
        }
        """;
        var elem = JsonDocument.Parse(json).RootElement;
        var geom = GeoJsonGeometryParser.ParsePolygonOrMultiPolygon(elem);
        Assert.That(geom, Is.Not.Null);
        Assert.That(geom!.SRID, Is.EqualTo(4326));
        Assert.That(geom, Is.InstanceOf<Polygon>());
    }

    [Test]
    public void Parse_Valid_MultiPolygon_Works()
    {
        var json = """
        {
          "type": "MultiPolygon",
          "coordinates": [
            [
              [
                [-122.55, 37.70],
                [-122.35, 37.70],
                [-122.35, 37.84],
                [-122.55, 37.84],
                [-122.55, 37.70]
              ]
            ],
            [
              [
                [-122.6, 37.65],
                [-122.4, 37.65],
                [-122.4, 37.75],
                [-122.6, 37.75],
                [-122.6, 37.65]
              ]
            ]
          ]
        }
        """;
        var elem = JsonDocument.Parse(json).RootElement;
        var geom = GeoJsonGeometryParser.ParsePolygonOrMultiPolygon(elem);
        Assert.That(geom, Is.Not.Null);
        Assert.That(geom!.SRID, Is.EqualTo(4326));
        Assert.That(geom, Is.InstanceOf<MultiPolygon>());
    }

    [Test]
    public void Parse_Invalid_GeoJson_Returns_Null()
    {
        var json = """
        { "type": "LineString", "coordinates": [[0,0],[1,1]] }
        """;
        var elem = JsonDocument.Parse(json).RootElement;
        var geom = GeoJsonGeometryParser.ParsePolygonOrMultiPolygon(elem);
        Assert.That(geom, Is.Null);
    }
}
