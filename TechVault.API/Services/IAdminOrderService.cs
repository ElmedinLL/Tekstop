using TechVault.API.Models.Enums;
using TechVault.API.Orders;

namespace TechVault.API.Services;

public interface IAdminOrderService
{
    /// <summary>Marks an order shipped, stores the tracking URL, and sends the shipped email.</summary>
    Task MarkOrderShippedAsync(int orderId, string trackingUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Paged admin list: optional status, placed-at range (<paramref name="fromPlacedUtc"/> inclusive,
    /// <paramref name="toPlacedUtc"/> inclusive calendar day, UTC), search by order id / order number / email substring.
    /// </summary>
    Task<AdminOrderListResult> ListOrdersAsync(
        OrderStatus? status,
        DateTime? fromPlacedUtc,
        DateTime? toPlacedUtc,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
