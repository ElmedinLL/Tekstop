namespace TechVault.API.Services;

public interface IAdminOrderService
{
    /// <summary>Marks an order shipped, stores the tracking URL, and sends the shipped email.</summary>
    Task MarkOrderShippedAsync(int orderId, string trackingUrl, CancellationToken cancellationToken = default);
}
