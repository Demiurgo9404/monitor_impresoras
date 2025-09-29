using MailKit.Net.Smtp;
using MimeKit;
using System.Text;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para envío de emails
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendReportEmailAsync(
            IEnumerable<string> recipients,
            string subject,
            string body,
            byte[] attachment,
            string attachmentFileName,
            string contentType)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["Email:FromName"] ?? "Sistema de Reportes",
                    _configuration["Email:FromAddress"] ?? "noreply@monitorimpresoras.com"));

                foreach (var recipient in recipients)
                {
                    message.To.Add(new MailboxAddress("", recipient));
                }

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"<p>{body}</p>",
                    TextBody = body
                };

                // Agregar adjunto
                bodyBuilder.Attachments.Add(attachmentFileName, attachment, ContentType.Parse(contentType));
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _configuration["Email:SmtpHost"] ?? "localhost",
                    int.Parse(_configuration["Email:SmtpPort"] ?? "25"),
                    bool.Parse(_configuration["Email:UseSsl"] ?? "false"));

                if (!string.IsNullOrEmpty(_configuration["Email:Username"]))
                {
                    await client.AuthenticateAsync(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email enviado exitosamente a {RecipientCount} destinatarios", recipients.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de reporte");
                throw;
            }
        }

        public async Task SendSimpleEmailAsync(string recipient, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["Email:FromName"] ?? "Sistema de Reportes",
                    _configuration["Email:FromAddress"] ?? "noreply@monitorimpresoras.com"));

                message.To.Add(new MailboxAddress("", recipient));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"<p>{body}</p>",
                    TextBody = body
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _configuration["Email:SmtpHost"] ?? "localhost",
                    int.Parse(_configuration["Email:SmtpPort"] ?? "25"),
                    bool.Parse(_configuration["Email:UseSsl"] ?? "false"));

                if (!string.IsNullOrEmpty(_configuration["Email:Username"]))
                {
                    await client.AuthenticateAsync(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email simple enviado exitosamente a: {Recipient}", recipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email simple");
                throw;
            }
        }
    }
}
