namespace TechVault.API.Auth;

public sealed class LoginDto
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}

