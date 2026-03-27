using TechVault.API.Orders;

namespace TechVault.API.Services;

public interface IOrderService
{
    Task<OrderDetailDto> CreateAsync(string identityUserId, CreateOrderDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderListItemDto>> ListAsync(string identityUserId, CancellationToken cancellationToken = default);

    Task<OrderDetailDto?> GetByIdAsync(string identityUserId, int orderId, CancellationToken cancellationToken = default);

    Task<OrderDetailDto?> CancelAsync(string identityUserId, int orderId, CancellationToken cancellationToken = default);
}
