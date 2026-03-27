using TechVault.API.Orders;

namespace TechVault.API.Services;

public interface IAdminOrderService
{
    Task<AdminOrderListResult> ListOrdersAsync(
        int page,
        string? status,
        string? fromDate,
        string? toDate,
        CancellationToken cancellationToken = default);

    Task<AdminOrderDetailDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);

    /// <summary>Marks an order shipped, stores the tracking URL, and sends the shipped email.</summary>
    Task MarkOrderShippedAsync(int orderId, string trackingUrl, CancellationToken cancellationToken = default);
}
