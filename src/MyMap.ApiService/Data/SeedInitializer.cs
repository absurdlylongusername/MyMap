using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Globalization;

namespace MyMap.ApiService.Data;

public sealed class SeedInitializer(NpgsqlDataSource dataSource,
                                    IDbContextFactory<AppDbContext> dbFactory,
                                    IOptions<SeedOptions> seedOptions,
                                    ILogger<SeedInitializer> log) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var seed    = seedOptions.Value;
        var enable  = seed.Enabled;
        var version = seed.Version;
        var dataDir = seed.DataDir;

        if (!enable || string.IsNullOrWhiteSpace(version))
        {
            log.LogInformation($"Seed disabled or {nameof(SeedOptions.Version)} missing. Skipping.");
            return;
        }

        // Ensure schema
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        await db.Database.MigrateAsync(ct);

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        // Ensure dataset_meta row exists
        await using (var upsertMeta = new NpgsqlCommand("""
            insert into dataset_meta (id, active_version)
            values (1, '')
            on conflict (id) do nothing;
            """, conn))
        {
            await upsertMeta.ExecuteNonQueryAsync(ct);
        }

        // Read active version
        var active = "";
        await using (var read = new NpgsqlCommand("select active_version from dataset_meta where id = 1;", conn))
        await using (var r = await read.ExecuteReaderAsync(ct))
        {
            if (await r.ReadAsync(ct)) active = r.GetString(0);
        }

        if (string.Equals(active, version, StringComparison.Ordinal))
        {
            log.LogInformation("Seed version {version} already active. Skipping.", version);
            return;
        }

        // Check for seed file
        var csv = Path.Combine(dataDir ?? "", $"pois_{version}.csv");
        if (!File.Exists(csv))
        {
            log.LogWarning("Seed file not found: {csv}", csv);
            return;
        }

        // Load CSV  batched inserts in one transaction
        var lines = File.ReadLines(csv).Skip(1); // skip header
        var count = 0;

        await using var transaction = await conn.BeginTransactionAsync(ct);

        foreach (var line in lines)
        {
            // Simple CSV split; OK for demo. Replace with CsvHelper for complex data.
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 5) continue; // TODO: remove magic number

            var name = parts[0];
            var category = parts[1];
            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)) continue;
            if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon)) continue;
            if (!DateTimeOffset.TryParse(parts[4], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var updated)) continue;

            await using var cmd = new NpgsqlCommand(
                """
                insert into pois (id, name, category, geom, updated_at_utc, data_version)
                values (@id, @name, @category, ST_SetSRID(ST_MakePoint(@lon, @lat), 4326), @updated, @version);
                """,
                conn,
                transaction
            );

            cmd.Parameters.AddWithValue("id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("category", category);
            cmd.Parameters.AddWithValue("lon", lon);
            cmd.Parameters.AddWithValue("lat", lat);
            cmd.Parameters.AddWithValue("updated", updated);
            cmd.Parameters.AddWithValue("version", version);

            await cmd.ExecuteNonQueryAsync(ct);
            count++;
        }

        // Upsert lineage and set active version
        await using (var upsertVersion = new NpgsqlCommand(
                         """
                         insert into dataset_versions (version, source, pulled_at_utc, transforms_json)
                         values (@v, 'seed-csv', now() at time zone 'utc', '{}'::jsonb)
                         on conflict (version) do update
                         set pulled_at_utc = excluded.pulled_at_utc;
                         """,
                         conn,
                         transaction
                     ))
        {
            upsertVersion.Parameters.AddWithValue("v", version);
            await upsertVersion.ExecuteNonQueryAsync(ct);
        }

        await using (var setActive = new NpgsqlCommand(
                         """
                         update dataset_meta
                         set active_version = @v
                         where id = 1;
                         """,
                         conn,
                         transaction
                     ))
        {
            setActive.Parameters.AddWithValue("v", version);
            await setActive.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
        log.LogInformation("Seeded {count} POIs for version {version}", count, version);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}