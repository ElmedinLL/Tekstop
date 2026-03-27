using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechVault.API.Inventory;
using TechVault.API.Models;
using TechVault.API.Products;
using TechVault.API.Repositories;

namespace TechVault.API.Services;

public sealed class LowStockInventoryService(
    IRepository<Product> productRepository,
    IOptions<InventorySettings> inventoryOptions,
    IMapper mapper) : ILowStockInventoryService
{
    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        var threshold = inventoryOptions.Value.LowStockThreshold;
        var products = await productRepository
            .QueryAsNoTracking()
            .Where(p => !p.IsDeleted && p.StockQuantity < threshold)
            .Include(p => p.Category)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<LowStockProductDto>>(products);
    }
}
