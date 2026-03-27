using Microsoft.EntityFrameworkCore;
using TechVault.API.Data;

namespace TechVault.API.Services;

public sealed class ProductReviewStatsService(ApplicationDbContext db) : IProductReviewStatsService
{
    public async Task RecalculateForProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
        {
            return;
        }

        var ratings = await db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Select(r => (int)r.Rating)
            .ToListAsync(cancellationToken);

        if (ratings.Count == 0)
        {
            product.AverageRating = null;
            product.ReviewCount = 0;
        }
        else
        {
            product.AverageRating = decimal.Round(
                (decimal)ratings.Average(),
                2,
                MidpointRounding.AwayFromZero);
            product.ReviewCount = ratings.Count;
        }

        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
