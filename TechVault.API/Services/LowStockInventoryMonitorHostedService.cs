using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechVault.API.Inventory;

namespace TechVault.API.Services;

/// <summary>Runs <see cref="ILowStockInventoryService"/> on a daily interval and logs results; optionally emails a summary.</summary>
public sealed class LowStockInventoryMonitorHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<InventorySettings> inventoryOptions,
    ILogger<LowStockInventoryMonitorHostedService> logger) : BackgroundService
{
    private readonly InventorySettings _settings = inventoryOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.EnableDailyLowStockCheck)
        {
            logger.LogInformation(
                "Daily low-stock check is disabled ({Section}:{Property}).",
                InventorySettings.SectionName,
                nameof(InventorySettings.EnableDailyLowStockCheck));
            return;
        }

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCheckAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Low-stock inventory check failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var inventory = scope.ServiceProvider.GetRequiredService<ILowStockInventoryService>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var items = await inventory.GetLowStockProductsAsync(cancellationToken);
        logger.LogInformation(
            "Low-stock check (stock below {Threshold}): {Count} product(s).",
            _settings.LowStockThreshold,
            items.Count);

        foreach (var p in items.Take(20))
        {
            logger.LogInformation(
                "Low stock: Id={Id} SKU={Sku} Stock={Stock} Published={Published} Name={Name}",
                p.Id,
                p.Sku,
                p.StockQuantity,
                p.IsPublished,
                p.Name);
        }

        if (items.Count > 20)
        {
            logger.LogInformation("Low-stock check: {More} additional product(s) not listed in logs.", items.Count - 20);
        }

        if (items.Count == 0 || string.IsNullOrWhiteSpace(_settings.LowStockAlertEmail))
        {
            return;
        }

        var body = new StringBuilder();
        body.AppendLine($"Low-stock report (stock < {_settings.LowStockThreshold}): {items.Count} product(s).");
        body.AppendLine();
        foreach (var p in items)
        {
            body.AppendLine($"{p.Sku}\t{p.StockQuantity}\t{p.CategoryName}\t{p.Name}");
        }

        await emailSender.SendAsync(
            _settings.LowStockAlertEmail.Trim(),
            $"[{items.Count}] TechVault low-stock products",
            body.ToString(),
            htmlBody: null,
            cancellationToken);
    }
}
