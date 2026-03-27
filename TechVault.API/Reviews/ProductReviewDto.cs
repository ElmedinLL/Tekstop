namespace TechVault.API.Reviews;

public sealed class ProductReviewDto
{
    public int Id { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string AuthorDisplayName { get; set; } = null!;
}
