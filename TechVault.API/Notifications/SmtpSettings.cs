namespace TechVault.API.Notifications;

public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "TechVault";
    public bool UseSsl { get; set; } = true;
}
