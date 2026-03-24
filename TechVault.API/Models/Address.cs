namespace TechVault.API.Models;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Label { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string City { get; set; } = null!;
    public string? Region { get; set; }
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string? Phone { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}
