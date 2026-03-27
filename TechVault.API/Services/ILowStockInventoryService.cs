using TechVault.API.Products;

namespace TechVault.API.Services;

public interface ILowStockInventoryService
{
    Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
}
