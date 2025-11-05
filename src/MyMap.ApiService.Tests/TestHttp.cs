using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyMap.ApiService.Tests;

public static class TestHttp
{
    public static HttpClient CreateClientNoCookies()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = false,
            AllowAutoRedirect = false
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = TestConfiguration.BaseUri
        };
        return client;
    }

    public static HttpClient CreateClientWithCookies(out CookieContainer cookieContainer)
    {
        cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookieContainer,
            AllowAutoRedirect = false
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = TestConfiguration.BaseUri
        };
        return client;
    }

    public static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email = "test@nimbus.local", string password = "Test123!")
    {
        var payload = JsonSerializer.Serialize(new { email, password });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        return await client.PostAsync(new Uri("/auth/login-cookie", UriKind.Relative), content);
    }
}
