using gasosa_backend.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace gasosa_backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetCodeAsync(string toEmail, string code)
        {
            var emailConfig = _configuration.GetSection("Email");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                emailConfig["FromName"] ?? "Gasosa App",
                emailConfig["Username"]
            ));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Código de recuperação de senha - Gasosa";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;"">
                        <div style=""background-color: #127D47; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: white; margin: 0;"">Gasosa</h1>
                        </div>
                        <div style=""padding: 30px; background-color: #f9f9f9;"">
                            <h2 style=""color: #333;"">Recuperação de senha</h2>
                            <p style=""color: #666;"">Você solicitou a recuperação de senha. Use o código abaixo para continuar:</p>
                            <div style=""background-color: #127D47; color: white; font-size: 36px; font-weight: bold; text-align: center; padding: 20px; border-radius: 8px; letter-spacing: 10px; margin: 20px 0;"">
                                {code}
                            </div>
                            <p style=""color: #666;"">Este código expira em <strong>15 minutos</strong>.</p>
                            <p style=""color: #999; font-size: 12px;"">Se você não solicitou a recuperação de senha, ignore este email.</p>
                        </div>
                    </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                emailConfig["Host"],
                int.Parse(emailConfig["Port"] ?? "587"),
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(emailConfig["Username"], emailConfig["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
