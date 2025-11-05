using System.Net;
using System.Text.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MyMap.ApiService.Tests;

public sealed class DatasetEndpointsTests
{
    [Test]
    public async Task Get_Active_Version_Unauthorized()
    {
        using var http = TestHttp.CreateClientNoCookies();
        using var resp = await http.GetAsync(new Uri("/api/datasets/version", UriKind.Relative));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Get_Active_Version_Authorized_Returns_200()
    {
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var login = await TestHttp.LoginAsync(http);
        Assume.That(login.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        using var resp = await http.GetAsync(new Uri("/api/datasets/version", UriKind.Relative));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.TryGetProperty("activeVersion", out _), "Response should contain 'activeVersion'");
    }

    [Test]
    public async Task Get_Version_By_Id_Behaves_As_Expected()
    {
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var login = await TestHttp.LoginAsync(http);
        Assume.That(login.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Query the active version first
        using var verResp = await http.GetAsync(new Uri("/api/datasets/version", UriKind.Relative));
        Assume.That(verResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var verJson = await verResp.Content.ReadAsStringAsync();
        using var verDoc = JsonDocument.Parse(verJson);
        var active = verDoc.RootElement.GetProperty("activeVersion").GetString();

        // Unknown version should be 404
        var unknown = Guid.NewGuid().ToString("N");
        using var notFoundResp = await http.GetAsync(new Uri($"/api/datasets/version/{unknown}", UriKind.Relative));
        Assert.That(notFoundResp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        // If there is an active version value, it should be retrievable
        if (!string.IsNullOrWhiteSpace(active))
        {
            using var found = await http.GetAsync(new Uri($"/api/datasets/version/{active}", UriKind.Relative));
            Assert.That(found.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        else
        {
            Assert.Inconclusive("No active version set yet.");
        }
    }
}
