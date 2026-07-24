using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace CSA.Services
{
    /// <summary>
    /// Central place for outbound email. SMTP settings come from Web.config
    /// (SmtpHost / SmtpPort / SmtpUser / SmtpPass / FromEmail). Sending is
    /// best-effort: when SMTP is not configured, or the send fails, the call
    /// returns false rather than throwing so callers can carry on.
    /// </summary>
    public static class EmailService
    {
        public static bool Send(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return false;

            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var fromEmail = ConfigurationManager.AppSettings["FromEmail"];
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
                return false; // Email not configured

            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
            var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            var smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            try
            {
                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    // Only enable SSL for remote hosts (Gmail, etc.), not local test relays.
                    client.EnableSsl = !smtpHost.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                        && !smtpHost.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

                    if (!string.IsNullOrEmpty(smtpUser))
                        client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    using (var mail = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "CyberShield Academy"),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    })
                    {
                        mail.To.Add(toEmail);
                        client.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
                return false;
            }
        }
    }
}
