using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TechVault.API.Notifications;

namespace TechVault.API.Services;

public sealed class MailKitEmailSender(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<MailKitEmailSender> logger) : IEmailSender
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task SendAsync(
        string toEmail,
        string subject,
        string plainTextBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host) || string.IsNullOrWhiteSpace(_smtp.FromEmail))
        {
            logger.LogInformation(
                "SMTP is not configured (Smtp:Host / Smtp:FromEmail). Email to {To} was not sent.",
                toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtp.FromName.Trim(), _smtp.FromEmail.Trim()));
        message.To.Add(MailboxAddress.Parse(toEmail.Trim()));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = plainTextBody
        };
        if (!string.IsNullOrEmpty(htmlBody))
        {
            builder.HtmlBody = htmlBody;
        }

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _smtp.Host.Trim(),
            _smtp.Port,
            SecureSocketOptions.Auto,
            cancellationToken);

        if (!string.IsNullOrEmpty(_smtp.UserName))
        {
            await client.AuthenticateAsync(_smtp.UserName, _smtp.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
