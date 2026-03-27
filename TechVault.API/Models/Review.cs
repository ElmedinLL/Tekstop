namespace TechVault.API.Models;

/// <summary>
/// Customer review for a product. At most one review per <see cref="UserId"/> and <see cref="ProductId"/>
/// (enforced by unique index). <see cref="Rating"/> must be 1–5 (database check constraint).
/// </summary>
public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    /// <summary>Star rating from 1 to 5.</summary>
    public byte Rating { get; set; }

    public string? Comment { get; set; }

    /// <summary>When the review was created (UTC).</summary>
    public DateTime CreatedAtUtc { get; set; }

    public string? Title { get; set; }

    public bool IsApproved { get; set; } = true;

    public DateTime? UpdatedAtUtc { get; set; }

    public Product Product { get; set; } = null!;

    public User User { get; set; } = null!;
}
