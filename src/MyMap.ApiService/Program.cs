using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMap.ApiService.Data;
using MyMap.ApiService.Endpoints;
using MyMap.ApiService.Setup;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Bind TestUserOptions with validation (use nameof for section key)
builder.Services
    .AddOptions<TestUserOptions>()
    .Bind(builder.Configuration.GetSection(nameof(TestUserOptions)))
    .ValidateDataAnnotations()
    .Validate(o => !o.Enabled || (!string.IsNullOrWhiteSpace(o.Email) && !string.IsNullOrWhiteSpace(o.Password)),
              "When Enabled, Email and Password must be provided.")
    .ValidateOnStart();

builder.ConfigureBuilder();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("web-client");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ConfigurePostgresLogging();
}

// Create a default test user if enabled in config
await app.EnsureDefaultTestUserAsync();

var auth = app.MapGroup("/auth");
auth.MapIdentityApi<IdentityUser>();
auth.MapPost(
        "/login-cookie",
        async (
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            LoginRequest request
        ) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email)
                       ?? await userManager.FindByNameAsync(request.Email);

            if (user is null)
            {
                return Results.BadRequest();
            }

            var result = await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: false
            );

            return result.Succeeded ? Results.Ok() : Results.BadRequest();
        }
    )
    .AllowAnonymous();

// Group for API endpoints; require auth
var api = app.MapGroup("/api").RequireAuthorization();

// Helper to set X-Data-Version per request
api.AddEndpointFilterFactory((context, next) => async invocation =>
{
    var serviceProvider = invocation.HttpContext.RequestServices;
    await using var scope = serviceProvider.CreateAsyncScope();
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

    await using var cmd = dataSource.CreateCommand(
        """
        select active_version
        from dataset_meta
        where id = 1
        """
    );

    string active = (string?)await cmd.ExecuteScalarAsync() ?? "";

    if (!string.IsNullOrEmpty(active))
    {
        invocation.HttpContext.Response.Headers["X-Data-Version"] = active;
        invocation.HttpContext.Items["ActiveDataVersion"] = active;
    }

    return await next(invocation);
});

// Map feature and dataset endpoints implemented via EF Core + NTS
api.MapFeatureEndpoints();
api.MapDatasetEndpoints();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

public sealed record LoginRequest(string Email, string Password);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
