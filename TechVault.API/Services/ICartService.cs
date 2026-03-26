using TechVault.API.Carts;

namespace TechVault.API.Services;

public interface ICartService
{
    Task<CartDto> GetAsync(CancellationToken cancellationToken = default);

    Task<CartDto> AddAsync(AddToCartDto dto, CancellationToken cancellationToken = default);

    Task<CartDto> UpdateAsync(UpdateCartDto dto, CancellationToken cancellationToken = default);
}
