using System.ComponentModel.DataAnnotations;

namespace TechVault.API.Reviews;

public sealed class CreateProductReviewDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(4000)]
    public string? Comment { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }
}
