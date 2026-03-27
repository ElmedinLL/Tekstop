using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using TechVault.API.Models;

namespace TechVault.API.Services;

public sealed class OrderNotificationService(
    IEmailSender emailSender,
    ILogger<OrderNotificationService> logger) : IOrderNotificationService
{
    public async Task SendOrderConfirmationAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.User is null || string.IsNullOrWhiteSpace(order.User.Email))
        {
            logger.LogWarning("Order {OrderId} has no user email; skipping confirmation email.", order.Id);
            return;
        }

        var to = order.User.Email.Trim();
        var subject = $"Your order {order.OrderNumber} is confirmed";
        var plain = BuildConfirmationPlainText(order);
        var html = BuildConfirmationHtml(order);

        await emailSender.SendAsync(to, subject, plain, html, cancellationToken);
    }

    public async Task SendOrderShippedAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.User is null || string.IsNullOrWhiteSpace(order.User.Email))
        {
            logger.LogWarning("Order {OrderId} has no user email; skipping shipped email.", order.Id);
            return;
        }

        if (string.IsNullOrWhiteSpace(order.TrackingUrl))
        {
            logger.LogWarning("Order {OrderId} has no tracking URL; skipping shipped email.", order.Id);
            return;
        }

        var to = order.User.Email.Trim();
        var subject = $"Your order {order.OrderNumber} has shipped";
        var plain = BuildShippedPlainText(order);
        var html = BuildShippedHtml(order);

        await emailSender.SendAsync(to, subject, plain, html, cancellationToken);
    }

    private static string BuildConfirmationPlainText(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Thank you for your order, {order.ShippingFullName}.");
        sb.AppendLine();
        sb.AppendLine($"Order number: {order.OrderNumber}");
        sb.AppendLine($"Placed (UTC): {order.PlacedAtUtc:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Subtotal: {order.SubTotal:F2} {order.Currency}");
        sb.AppendLine($"Shipping: {order.ShippingAmount:F2} {order.Currency}");
        if (order.TaxAmount > 0)
        {
            sb.AppendLine($"Tax: {order.TaxAmount:F2} {order.Currency}");
        }

        if (order.DiscountAmount > 0)
        {
            sb.AppendLine($"Discount: -{order.DiscountAmount:F2} {order.Currency}");
        }

        sb.AppendLine($"Total: {order.Total:F2} {order.Currency}");
        if (!string.IsNullOrEmpty(order.PaymentMethod))
        {
            sb.AppendLine($"Payment method: {order.PaymentMethod}");
        }

        sb.AppendLine();
        sb.AppendLine("Items:");
        foreach (var line in order.OrderItems)
        {
            sb.AppendLine(
                $"  - {line.ProductName} (SKU {line.ProductSku}) x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2} {order.Currency}");
        }

        sb.AppendLine();
        sb.AppendLine("Ship to:");
        sb.AppendLine(order.ShippingFullName);
        sb.AppendLine(order.ShippingLine1);
        if (!string.IsNullOrEmpty(order.ShippingLine2))
        {
            sb.AppendLine(order.ShippingLine2);
        }

        sb.AppendLine($"{order.ShippingCity}, {order.ShippingRegion} {order.ShippingPostalCode}");
        sb.AppendLine(order.ShippingCountry);
        if (!string.IsNullOrEmpty(order.ShippingPhone))
        {
            sb.AppendLine(order.ShippingPhone);
        }

        return sb.ToString();
    }

    private static string BuildConfirmationHtml(Order order)
    {
        var rows = new StringBuilder();
        foreach (var line in order.OrderItems)
        {
            rows.Append("<tr>");
            rows.Append($"<td>{WebUtility.HtmlEncode(line.ProductName)}</td>");
            rows.Append($"<td style=\"text-align:right\">{line.Quantity}</td>");
            rows.Append($"<td style=\"text-align:right\">{line.UnitPrice:F2}</td>");
            rows.Append($"<td style=\"text-align:right\">{line.LineTotal:F2}</td>");
            rows.Append("</tr>");
        }

        var ship2 = string.IsNullOrEmpty(order.ShippingLine2)
            ? ""
            : WebUtility.HtmlEncode(order.ShippingLine2) + "<br/>";

        var phone = string.IsNullOrEmpty(order.ShippingPhone)
            ? ""
            : $"<br/>{WebUtility.HtmlEncode(order.ShippingPhone)}";

        return $"""
            <html><body style="font-family:system-ui,sans-serif;line-height:1.5">
            <p>Thank you for your order, {WebUtility.HtmlEncode(order.ShippingFullName)}.</p>
            <p><strong>Order {WebUtility.HtmlEncode(order.OrderNumber)}</strong><br/>
            Placed (UTC): {WebUtility.HtmlEncode(order.PlacedAtUtc.ToString("yyyy-MM-dd HH:mm"))}</p>
            <table style="border-collapse:collapse;margin:1em 0">
            <tr><td>Subtotal</td><td style="text-align:right">{order.SubTotal:F2} {WebUtility.HtmlEncode(order.Currency)}</td></tr>
            <tr><td>Shipping</td><td style="text-align:right">{order.ShippingAmount:F2} {WebUtility.HtmlEncode(order.Currency)}</td></tr>
            {(order.TaxAmount > 0 ? $"<tr><td>Tax</td><td style=\"text-align:right\">{order.TaxAmount:F2} {WebUtility.HtmlEncode(order.Currency)}</td></tr>" : "")}
            {(order.DiscountAmount > 0 ? $"<tr><td>Discount</td><td style=\"text-align:right\">-{order.DiscountAmount:F2} {WebUtility.HtmlEncode(order.Currency)}</td></tr>" : "")}
            <tr><td><strong>Total</strong></td><td style="text-align:right"><strong>{order.Total:F2} {WebUtility.HtmlEncode(order.Currency)}</strong></td></tr>
            </table>
            {(string.IsNullOrEmpty(order.PaymentMethod) ? "" : $"<p>Payment method: {WebUtility.HtmlEncode(order.PaymentMethod)}</p>")}
            <table border="1" cellpadding="8" cellspacing="0" style="border-collapse:collapse">
            <thead><tr><th>Item</th><th>Qty</th><th>Unit</th><th>Line</th></tr></thead>
            <tbody>{rows}</tbody>
            </table>
            <p><strong>Ship to</strong><br/>
            {WebUtility.HtmlEncode(order.ShippingFullName)}<br/>
            {WebUtility.HtmlEncode(order.ShippingLine1)}<br/>
            {ship2}
            {WebUtility.HtmlEncode(order.ShippingCity)}, {WebUtility.HtmlEncode(order.ShippingRegion ?? "")} {WebUtility.HtmlEncode(order.ShippingPostalCode)}<br/>
            {WebUtility.HtmlEncode(order.ShippingCountry)}{phone}
            </p>
            </body></html>
            """;
    }

    private static string BuildShippedPlainText(Order order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Hi {order.ShippingFullName},");
        sb.AppendLine();
        sb.AppendLine($"Your order {order.OrderNumber} has shipped.");
        sb.AppendLine();
        sb.AppendLine($"Track your shipment: {order.TrackingUrl}");
        sb.AppendLine();
        sb.AppendLine("Items in this order:");
        foreach (var line in order.OrderItems)
        {
            sb.AppendLine($"  - {line.ProductName} x{line.Quantity}");
        }

        return sb.ToString();
    }

    private static string BuildShippedHtml(Order order)
    {
        var trackingHref = WebUtility.HtmlEncode(order.TrackingUrl!);
        var rows = new StringBuilder();
        foreach (var line in order.OrderItems)
        {
            rows.Append("<tr>");
            rows.Append($"<td>{WebUtility.HtmlEncode(line.ProductName)}</td>");
            rows.Append($"<td style=\"text-align:right\">{line.Quantity}</td>");
            rows.Append("</tr>");
        }

        return $"""
            <html><body style="font-family:system-ui,sans-serif;line-height:1.5">
            <p>Hi {WebUtility.HtmlEncode(order.ShippingFullName)},</p>
            <p>Your order <strong>{WebUtility.HtmlEncode(order.OrderNumber)}</strong> has shipped.</p>
            <p><a href="{trackingHref}">Track your shipment</a></p>
            <table border="1" cellpadding="8" cellspacing="0" style="border-collapse:collapse;margin-top:1em">
            <thead><tr><th>Item</th><th>Qty</th></tr></thead>
            <tbody>{rows}</tbody>
            </table>
            </body></html>
            """;
    }
}
