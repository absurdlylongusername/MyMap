namespace MyMap.ApiService.Types;

public sealed record FeatureDto(
    Guid Id,
    string Name,
    string Category,
    double Lat,
    double Lon,
    DateTimeOffset UpdatedAtUtc
);
