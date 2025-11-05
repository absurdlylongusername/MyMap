using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using static MyMap.ApiService.Data.Constants;

namespace MyMap.ApiService.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Poi> Pois => Set<Poi>();
    public DbSet<DatasetMeta> DatasetMeta => Set<DatasetMeta>();
    public DbSet<DatasetVersion> DatasetVersions => Set<DatasetVersion>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Spatial index must stay in fluent config (no annotation for GIST)
        b.Entity<Poi>().HasIndex(x => x.Geom).HasMethod("GIST");

        // Seed single row for dataset_meta
        b.Entity<DatasetMeta>().HasData(new DatasetMeta { Id = 1, ActiveVersion = "" });
    }
}

public sealed class Poi
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(512)]
    public string Name { get; set; } = "";

    [Required, MaxLength(64)]
    public string Category { get; set; } = "";

    [Required]
    [Column(TypeName = "geometry(Point,4326)")]
    public Point Geom { get; set; } = default!;

    [Required]
    public DateTimeOffset UpdatedAtUtc { get; set; }

    [Required, MaxLength(DataVersionMaxLength)]
    public string DataVersion { get; set; } = "";
}

public sealed class DatasetMeta
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(DataVersionMaxLength)]
    public string ActiveVersion { get; set; } = "";
}

public sealed class DatasetVersion
{
    [Key, MaxLength(DataVersionMaxLength)]
    public string Version { get; set; } = "";

    [Required, MaxLength(512)]
    public string Source { get; set; } = "";

    public DateTimeOffset? PulledAtUtc { get; set; }

    [Column(TypeName = "jsonb")]
    public string? TransformsJson { get; set; }
}