namespace MyMap.ApiService.Setup;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}
