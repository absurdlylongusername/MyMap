using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MyMap.ApiService.Setup;

public static class DefaultUserSetup
{
	public static async Task EnsureDefaultTestUserAsync(this WebApplication app)
	{
        var options = app.Services.GetRequiredService<IOptions<TestUserOptions>>().Value;
		if (!options.Enabled) return;
        await using var scope = app.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

		var existingUser = await userManager.FindByEmailAsync(options.Email) ?? await userManager.FindByNameAsync(options.Email);
		if (existingUser is not null) return;

		var user = new IdentityUser
		{
			Email = options.Email,
			UserName = options.Email,
			EmailConfirmed = true
		};

		var createResult = await userManager.CreateAsync(user, options.Password);
		if (!createResult.Succeeded)
		{
			var errors = string.Join(", ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
			throw new InvalidOperationException($"Failed to create default user: {errors}");
		}
		else
		{
			app.Logger.LogInformation("Created default test user {Email}", options.Email);
        }
    }
}
