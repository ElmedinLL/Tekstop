using TechVault.API.Models;

namespace TechVault.API.Services;

public interface IProductService
{
    Task<Product> CreateAsync(CreateProductInput input, CancellationToken cancellationToken = default);

    Task<Product> UpdateAsync(int productId, UpdateProductInput input, CancellationToken cancellationToken = default);
}
