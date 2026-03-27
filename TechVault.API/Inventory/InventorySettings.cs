namespace TechVault.API.Inventory;

public sealed class InventorySettings
{
    public const string SectionName = "Inventory";

    /// <summary>Products with stock strictly below this value are low stock (default 10).</summary>
    public int LowStockThreshold { get; init; } = 10;

    /// <summary>When true, runs a background check on a daily interval after startup.</summary>
    public bool EnableDailyLowStockCheck { get; init; } = true;

    /// <summary>Optional recipient for a daily plain-text summary when products are low stock. SMTP must be configured.</summary>
    public string? LowStockAlertEmail { get; init; }
}
