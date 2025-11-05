namespace MyMap.ApiService.Data;

public sealed class DatabaseOptions
{
    public string ResourceName { get; set; } = "mymapdb";
}

public sealed class SeedOptions
{
    public bool   Enabled { get; set; }
    public string Version { get; set; } = "";
    public string DataDir { get; set; } = "";
}

public sealed class PgLoggingOptions
{
    public bool LogUser { get; set; }
}