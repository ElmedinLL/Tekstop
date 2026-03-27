using TechVault.API.Models;

namespace TechVault.API.Services;

public interface IOrderNotificationService
{
    Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default);

    Task SendOrderShippedAsync(Order order, CancellationToken cancellationToken = default);
}
