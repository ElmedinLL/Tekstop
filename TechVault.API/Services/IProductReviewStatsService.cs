namespace TechVault.API.Services;

/// <summary>Recomputes denormalized review aggregates on <see cref="Models.Product"/> from approved reviews.</summary>
public interface IProductReviewStatsService
{
    Task RecalculateForProductAsync(int productId, CancellationToken cancellationToken = default);
}
