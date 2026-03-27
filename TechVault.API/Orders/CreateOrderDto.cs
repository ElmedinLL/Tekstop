namespace TechVault.API.Orders;

public sealed class CreateOrderDto
{
    public string ShippingFullName { get; set; } = null!;
    public string ShippingLine1 { get; set; } = null!;
    public string? ShippingLine2 { get; set; }
    public string ShippingCity { get; set; } = null!;
    public string? ShippingRegion { get; set; }
    public string ShippingPostalCode { get; set; } = null!;
    public string ShippingCountry { get; set; } = null!;
    public string? ShippingPhone { get; set; }
    public string BillingFullName { get; set; } = null!;
    public string BillingLine1 { get; set; } = null!;
    public string? BillingLine2 { get; set; }
    public string BillingCity { get; set; } = null!;
    public string? BillingRegion { get; set; }
    public string BillingPostalCode { get; set; } = null!;
    public string BillingCountry { get; set; } = null!;
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
}
