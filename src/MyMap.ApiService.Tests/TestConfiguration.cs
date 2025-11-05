using Microsoft.Extensions.Configuration;
using System;

namespace MyMap.ApiService.Tests;

public static class TestConfiguration
{
    static TestConfiguration()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("testsettings.json", optional: false)
            .Build();

        Options = Configuration.GetRequiredSection(nameof(TestOptions)).Get<TestOptions>()
                   ?? new TestOptions();

        if (string.IsNullOrWhiteSpace(Options.ApiBaseUrl))
        {
            throw new InvalidOperationException("ApiBaseUrl is missing in testsettings.json");
        }

        BaseUri = new Uri(Options.ApiBaseUrl);
    }

    public static IConfigurationRoot Configuration { get; }
    public static TestOptions Options { get; }
    public static Uri BaseUri { get; }
}
