namespace TechVault.API.Reviews;

public sealed class CreateProductReviewDto
{
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public string? Title { get; set; }
}
