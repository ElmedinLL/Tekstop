using System;
using Microsoft.AspNetCore.Identity;

namespace TechVault.API.Auth;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ProfilePicture { get; set; }
}