using TechVault.API.Carts;

namespace TechVault.API.Services;

public interface ICartService
{
    Task<CartDto> GetCart(string userId, CancellationToken cancellationToken = default);

    Task<CartDto> AddItem(string userId, int productId, int qty, CancellationToken cancellationToken = default);

    Task<CartDto> UpdateQty(string userId, int itemId, int qty, CancellationToken cancellationToken = default);

    Task<CartDto> RemoveItem(string userId, int itemId, CancellationToken cancellationToken = default);

    Task<CartDto> ClearCart(string userId, CancellationToken cancellationToken = default);
}
