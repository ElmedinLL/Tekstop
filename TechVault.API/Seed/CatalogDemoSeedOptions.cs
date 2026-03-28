namespace TechVault.API.Seed;

/// <summary>Optional startup seed: demo products per category (development / local DB).</summary>
public sealed class CatalogDemoSeedOptions
{
    public const string SectionName = "CatalogDemoSeed";

    /// <summary>When true, ensures each category has up to <see cref="ProductsPerCategory"/> demo SKUs (prefix <see cref="SkuPrefix"/>).</summary>
    public bool Enabled { get; set; }

    /// <summary>Target count of demo products per category (existing demo SKUs are counted).</summary>
    public int ProductsPerCategory { get; set; } = 4;

    /// <summary>Optional seed for repeatable random names/prices; omit for non-deterministic runs.</summary>
    public int? RandomSeed { get; set; }

    /// <summary>SKU prefix for demo rows (e.g. DEMO → DEMO-12-A1B2C3D4).</summary>
    public string SkuPrefix { get; set; } = "DEMO";
}
