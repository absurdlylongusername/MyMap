using Microsoft.Extensions.Options;
using MyMap.ApiService.Data;
using Npgsql;

namespace MyMap.ApiService.Setup;

public static class ApplicationSetup
{
    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        return app;
    }

    public static async Task<WebApplication> ConfigurePostgresLogging(this WebApplication app)
    {
        // Role-level PG logging toggle via options
        var pgLog = app.Services.GetRequiredService<IOptions<PgLoggingOptions>>().Value;
        if (pgLog.LogUser)
        {
            using var scope = app.Services.CreateScope();
            var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
            await using var conn = await dataSource.OpenConnectionAsync();

            await using (var cmd = new NpgsqlCommand("ALTER ROLE postgres SET log_statement = 'all';", conn))
                await cmd.ExecuteNonQueryAsync();
            await using (var cmd = new NpgsqlCommand("ALTER ROLE postgres SET log_min_duration_statement = 0;", conn))
                await cmd.ExecuteNonQueryAsync();
            await using (var cmd = new NpgsqlCommand("SELECT pg_reload_conf();", conn))
                await cmd.ExecuteNonQueryAsync();
        }
        return app;
    }
}
