using Microsoft.Extensions.Configuration;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var postgresOptions = builder.Configuration.GetRequiredSection(PostgresOptions.SectionName)
    .Get<PostgresOptions>() ?? throw new InvalidOperationException($"Invalid configuration for {PostgresOptions.SectionName}");

// Use a PostGIS-enabled image and create a database resource
var postgres = builder
    .AddPostgres("postgres")
    .WithImage("postgis/postgis", tag: postgresOptions.ImageTag)
    .WithEnvironment("POSTGRES_PASSWORD", postgresOptions.Password)
    .WithDataVolume(postgresOptions.DataVolume);

var myMapDb = postgres.AddDatabase(postgresOptions.Database);

var apiService = builder.AddProject<Projects.MyMap_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(myMapDb)
    .WaitFor(myMapDb);

var scalar = builder.AddScalarApiReference();
scalar.WithApiReference(apiService);

//var frontendName = "frontend";
//builder.AddViteApp(frontendName, "../MyMap.Web")
//    .WithReference(apiService)
//    .WaitFor(apiService)
//    .WithNpmPackageInstallation()
//    .WithExternalHttpEndpoints();

builder.Build().Run();

file sealed record PostgresOptions(string Password,
                                   string ImageTag,
                                   string Database,
                                   string DataVolume)
{
  public const string SectionName = "Infrastructure:Postgres";
}
