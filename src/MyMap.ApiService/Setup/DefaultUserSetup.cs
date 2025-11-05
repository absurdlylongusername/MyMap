using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MyMap.ApiService.Setup;

public static class DefaultUserSetup
{
    public static async Task EnsureDefaultTestUserAsync(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<TestUserOptions>>().Value;
        if (!options.Enabled || string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var existingUser = await userManager.FindByEmailAsync(options.Email);
        if (existingUser is not null)
        {
            return;
        }

        var user = new IdentityUser
        {
            UserName = options.Email,
            Email = options.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, options.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
            app.Logger.LogWarning("Failed to create default test user: {Errors}", errors);
        }
        else
        {
            app.Logger.LogInformation("Created default test user {Email}", options.Email);
        }
    }
}
