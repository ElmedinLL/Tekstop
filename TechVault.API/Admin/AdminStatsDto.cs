namespace TechVault.API.Admin;

/// <summary>Aggregate metrics for the admin dashboard.</summary>
public sealed class AdminStatsDto
{
    /// <summary>Sum of order totals for orders that count as revenue (excludes cancelled, refunded, pending payment).</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>All orders in the system.</summary>
    public int TotalOrders { get; set; }

    /// <summary>Registered Identity users (AspNetUsers).</summary>
    public int TotalUsers { get; set; }

    /// <summary>Non-deleted catalog products.</summary>
    public int ProductsCount { get; set; }

    /// <summary>Revenue per calendar month (UTC), typically last 12 months including current.</summary>
    public IReadOnlyList<RevenueByMonthDto> RevenueByMonth { get; set; } = Array.Empty<RevenueByMonthDto>();
}

public sealed class RevenueByMonthDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    /// <summary>Revenue for this month (same rules as <see cref="AdminStatsDto.TotalRevenue"/>).</summary>
    public decimal Revenue { get; set; }
}
