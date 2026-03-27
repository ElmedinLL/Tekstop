namespace TechVault.API.Reviews;

/// <summary>Whether the current user bought this product and their review, if any.</summary>
public sealed class MyReviewStatusDto
{
    public bool Purchased { get; set; }

    public ProductReviewDto? Review { get; set; }
}
