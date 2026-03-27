using Microsoft.EntityFrameworkCore;
using TechVault.API.Admin;
using TechVault.API.Data;
using TechVault.API.Models.Enums;

namespace TechVault.API.Services;

public sealed class AdminStatsService(
    ApplicationDbContext db,
    AuthDbContext authDb) : IAdminStatsService
{
    /// <summary>Orders that represent realized or in-flight sales (not abandoned checkout, not voided).</summary>
    private static bool CountsTowardRevenue(OrderStatus status) =>
        status is not OrderStatus.Cancelled and not OrderStatus.Refunded and not OrderStatus.PendingPayment;

    public async Task<AdminStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await authDb.Users.CountAsync(cancellationToken);
        var productsCount = await db.Products.CountAsync(p => !p.IsDeleted, cancellationToken);
        var totalOrders = await db.Orders.CountAsync(cancellationToken);

        var totalRevenue = await db.Orders
            .AsNoTracking()
            .Where(o => CountsTowardRevenue(o.Status))
            .SumAsync(o => o.Total, cancellationToken);

        var revenueByMonth = await BuildRevenueByMonthAsync(cancellationToken);

        return new AdminStatsDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalUsers = totalUsers,
            ProductsCount = productsCount,
            RevenueByMonth = revenueByMonth
        };
    }

    /// <summary>Last 12 calendar months (UTC), including current month; months with no orders show 0.</summary>
    private async Task<IReadOnlyList<RevenueByMonthDto>> BuildRevenueByMonthAsync(CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var startMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

        var fromInclusive = startMonth;
        var toExclusive = startMonth.AddMonths(12);

        var aggregates = await db.Orders
            .AsNoTracking()
            .Where(o => o.PlacedAtUtc >= fromInclusive && o.PlacedAtUtc < toExclusive && CountsTowardRevenue(o.Status))
            .GroupBy(o => new { o.PlacedAtUtc.Year, o.PlacedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Total) })
            .ToListAsync(cancellationToken);

        var lookup = aggregates.ToDictionary(x => (x.Year, x.Month), x => x.Revenue);

        var result = new List<RevenueByMonthDto>(12);
        for (var i = 0; i < 12; i++)
        {
            var d = startMonth.AddMonths(i);
            var year = d.Year;
            var month = d.Month;
            var revenue = lookup.TryGetValue((year, month), out var r) ? r : 0m;
            result.Add(new RevenueByMonthDto { Year = year, Month = month, Revenue = revenue });
        }

        return result;
    }
}
