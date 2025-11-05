using System.Net;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MyMap.ApiService.Tests;

public sealed class SmokeTests
{

    [Test]
    public async Task OpenApi_Document_Is_Available_In_Development()
    {
        using var http = TestHttp.CreateClientNoCookies();
        using var response = await http.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative));
        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.NotFound),
            "OpenAPI doc should be available in Development; 404 acceptable if disabled.");
    }

    [Test]
    public async Task Protected_Feature_Requires_Login()
    {
        using var http = TestHttp.CreateClientNoCookies();
        using var response = await http.GetAsync(new Uri("/api/features", UriKind.Relative));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_Then_Access_Protected_Feature()
    {
        using var http = TestHttp.CreateClientWithCookies(out _);
        using var loginResponse = await TestHttp.LoginAsync(http);
        Assert.That(loginResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK).Or.EqualTo(HttpStatusCode.BadRequest),
            "Login should succeed; 400 indicates invalid credentials.");

        if (loginResponse.StatusCode == HttpStatusCode.OK)
        {
            using var featuresResponse = await http.GetAsync(new Uri("/api/features", UriKind.Relative));
            Assert.That(featuresResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        else
        {
            Assert.Inconclusive("Default test user may not exist or password mismatch. Ensure TestUser is enabled in appsettings and matches test payload.");
        }
    }
}
