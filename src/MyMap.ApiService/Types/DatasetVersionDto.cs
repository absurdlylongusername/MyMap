namespace MyMap.ApiService.Types;

public sealed record DatasetVersionDto(
    string Version,
    string Source,
    DateTimeOffset? PulledAtUtc,
    string? TransformsJson
);
