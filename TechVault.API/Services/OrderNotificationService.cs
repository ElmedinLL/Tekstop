using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TechVault.API.Models;
using TechVault.API.Notifications;

namespace TechVault.API.Services;

public sealed class OrderNotificationService(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<OrderNotificationService> logger) : IOrderNotificationService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.User is null || string.IsNullOrWhiteSpace(order.User.Email))
        {
            logger.LogWarning("Order {OrderId} has no user email; skipping confirmation email.", order.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(_smtp.Host) || string.IsNullOrWhiteSpace(_smtp.FromEmail))
        {
            logger.LogInformation(
                "SMTP is not configured (Smtp:Host / Smtp:FromEmail). Skipping order confirmation email for order {OrderNumber}.",
                order.OrderNumber);
            return;
        }

        var to = order.User.Email.Trim();
        var subject = $"Your order {order.OrderNumber} is confirmed";
        var body = BuildBody(order);

        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.FromEmail.Trim(), _smtp.FromName.Trim()),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(to));

        using var client = new SmtpClient(_smtp.Host.Trim(), _smtp.Port)
        {
            EnableSsl = _smtp.UseSsl
        };
        if (!string.IsNullOrEmpty(_smtp.UserName))
        {
            client.Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }

    private static string BuildBody(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Thank you for your order, {order.ShippingFullName}.");
        sb.AppendLine();
        sb.AppendLine($"Order number: {order.OrderNumber}");
        sb.AppendLine($"Total: {order.Total:F2} {order.Currency}");
        sb.AppendLine();
        sb.AppendLine("Items:");
        foreach (var line in order.OrderItems)
        {
            sb.AppendLine($"  - {line.ProductName} x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2}");
        }

        sb.AppendLine();
        sb.AppendLine("Shipping address:");
        sb.AppendLine($"{order.ShippingLine1}");
        if (!string.IsNullOrEmpty(order.ShippingLine2))
        {
            sb.AppendLine(order.ShippingLine2);
        }

        sb.AppendLine($"{order.ShippingCity}, {order.ShippingRegion} {order.ShippingPostalCode}");
        sb.AppendLine(order.ShippingCountry);
        return sb.ToString();
    }
}
