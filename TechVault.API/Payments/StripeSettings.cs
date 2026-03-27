namespace TechVault.API.Payments;

public sealed class StripeSettings
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; init; } = null!;
    public string WebhookSecret { get; init; } = "";
    public string PublishableKey { get; init; } = "";
    public string Currency { get; init; } = "usd";
}
