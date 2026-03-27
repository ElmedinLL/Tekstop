using TechVault.API.Orders;

namespace TechVault.API.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(string identityUserId, CreateOrderDto dto, CancellationToken cancellationToken = default);

    Task<OrderListResult> GetOrdersAsync(string identityUserId, int page, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderByIdAsync(string identityUserId, int orderId, CancellationToken cancellationToken = default);

    Task CancelOrderAsync(string identityUserId, int orderId, CancellationToken cancellationToken = default);
}
