using System.Net;
using System.Text;
using NUnit.Framework;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace MyMap.ApiService.Tests;

public sealed class FeatureEndpointsTests
{

    [Test]
    public async Task Get_Features_No_Bbox_Returns_Empty_Array()
    {
        using var httpNoCookies = TestHttp.CreateClientNoCookies();
        // Ensure we are unauthorized first
        var unauthorized = await httpNoCookies.GetAsync(new Uri("/api/features", UriKind.Relative));
        Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        // Login
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var loginResponse = await TestHttp.LoginAsync(http);
        Assume.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login must succeed for endpoint tests.");

        using var resp = await http.GetAsync(new Uri("/api/features", UriKind.Relative));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await resp.Content.ReadAsStringAsync();
        // Must be an array
        Assert.That(json.TrimStart().StartsWith("["));
    }

    [Test]
    public async Task Get_Features_Invalid_Bbox_Returns_400()
    {
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var loginResponse = await TestHttp.LoginAsync(http);
        Assume.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login must succeed for endpoint tests.");

        using var resp = await http.GetAsync(new Uri("/api/features?bbox=not-a-bbox", UriKind.Relative));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Post_Polygon_Missing_Body_Returns_400()
    {
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var loginResponse = await TestHttp.LoginAsync(http);
        Assume.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login must succeed for endpoint tests.");

        // Empty content triggers 400
        using var resp = await http.PostAsync(new Uri("/api/features/query", UriKind.Relative), new StringContent("", Encoding.UTF8, "application/json"));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Post_Polygon_Valid_Returns_200()
    {
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var loginResponse = await TestHttp.LoginAsync(http);
        Assume.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login must succeed for endpoint tests.");

        var poly = """
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
        using var polyContent = new StringContent(poly, Encoding.UTF8, "application/json");
        using var resp = await http.PostAsync(new Uri("/api/features/query?limit=10", UriKind.Relative), polyContent);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body.TrimStart().StartsWith("["));
    }
}
