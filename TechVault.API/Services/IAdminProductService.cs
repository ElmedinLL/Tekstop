using TechVault.API.Models;
using TechVault.API.Products;

namespace TechVault.API.Services;

public interface IAdminProductService
{
    Task<Product> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    Task<Product?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
}
