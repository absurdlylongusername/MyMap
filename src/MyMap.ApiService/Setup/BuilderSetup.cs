using Microsoft.AspNetCore.Identity;
using MyMap.ApiService.Data;
using Npgsql;

namespace MyMap.ApiService.Setup;

public static class BuilderSetup
{
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        // Bind options (config-driven)
        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(Constants.DatabaseSection));
        builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection(Constants.SeedSection));
        builder.Services.Configure<PgLoggingOptions>(builder.Configuration.GetSection(Constants.PgLoggingSection));
        builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(nameof(CorsOptions)));

        // Add service defaults (observability, health, etc.)
        builder.AddServiceDefaults();

        // Build an NpgsqlDataSource configured with NetTopologySuite using the Aspire-provided connection string
        var dbOptions = builder.Configuration.GetSection(Constants.DatabaseSection).Get<DatabaseOptions>()!;
        var connString = builder.Configuration.GetConnectionString(dbOptions.ResourceName)
                         ?? throw new InvalidOperationException($"Missing connection string for '{dbOptions.ResourceName}'.");

        builder.Services.AddSingleton(sp =>
        {
            var dsb = new NpgsqlDataSourceBuilder(connString);
            dsb.UseNetTopologySuite();
            return dsb.Build();
        });

        // EF Core (shared provider conventions)
        builder.Services.AddDbContextPool<AppDbContext>((serviceProvider, options) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            options.ConfigureDbContextOptions(dataSource);
        });

        builder.Services.AddDbContextFactory<AppDbContext>((serviceProvider, options) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            options.ConfigureDbContextOptions(dataSource);
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        })
        .AddIdentityCookies();

        // For APIs: return 401/403 instead of 302 redirects
        builder.Services.ConfigureApplicationCookie(options =>
        {
            // API-friendly: return 401/403, not redirects
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };

            // Cross-site dev (React/Vite) requires SameSite=None and Secure
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
        });

        builder.Services.AddAuthorization();
        builder.Services.AddIdentityCore<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();

        // CORS
        // TODO: get options properly since it's registered at the top of this file
        // 
        var cors = builder.Configuration.GetSection(nameof(CorsOptions)).Get<CorsOptions>() ?? new CorsOptions();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("web-client", policy =>
            {
                if (cors.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(cors.AllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            });
        });

        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();

        // Hosted seed initializer (uses options)
        builder.Services.AddHostedService<SeedInitializer>();

        return builder;
    }
}
