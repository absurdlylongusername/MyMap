using System.ComponentModel.DataAnnotations;

namespace MyMap.ApiService.Setup;

public sealed class TestUserOptions
{
	public bool Enabled { get; set; } = false;

	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[MinLength(6)]
	public string Password { get; set; } = string.Empty;
}
