using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Globalization;
using Reservation.Shared.Common;

namespace Reservation.Api.Services
{
    public class EmailService : IEmailService
    {
        private const string SenderEmail = "info@rezervario.cz";
        private const string SenderName = "Rezervario";
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendRegistrationSuccessEmailAsync(string recipientEmail, string firstName, string lastName)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Vítejte v Rezervario!",
                "Registrace úspěšná",
                "#007BFF",
                "Vítejte v Rezervario!",
                "Děkujeme vám za registraci v naší aplikaci. Vaše registrace proběhla úspěšně a nyní můžete využívat všechny výhody naší platformy."
            );
        }

        public async Task SendReservationCancellationByUserEmailAsync(string recipientEmail, string firstName, string lastName,
            string reservationTitle, DateTime reservationDate, CultureInfo cultureInfo)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Rezervace byla zrušena",
                "Zrušení rezervace",
                "#d9534f",
                "Rezervace byla zrušena",
                $"tímto Vás informujeme, že rezervace <strong>\"{reservationTitle}\"</strong> plánovaná na {reservationDate.ToString("f", cultureInfo)} byla zrušena."
            );
        }

        public async Task SendReservationConfirmationEmailWithUnsubscribeLinkAsync(
            string recipientEmail,
            string firstName,
            string lastName,
            string reservationTitle,
            DateTime reservationDate,
            TimeSpan duration,
            int reservationId,
            string unsubscribeLink,
            CultureInfo cultureInfo)
        {
            string additionalContent = $@"<p>Pokud se chcete od této rezervace odhlásit, klikněte na následující odkaz:</p>
            <p style=""text-align: center;"">
                <a href=""https://www.rezervario.cz/zruseni-rezervace/{reservationId}/{unsubscribeLink}"" 
                   style=""display: inline-block; padding: 10px 20px; color: #fff; background-color: #d9534f; 
                   text-decoration: none; border-radius: 4px;"">Odhlásit rezervaci</a>
            </p>";

            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Potvrzení rezervace",
                "Potvrzení rezervace",
                "#28a745",
                "Rezervace úspěšná",
                $"Vaše rezervace <strong>{reservationTitle}</strong> na datum {reservationDate.ToString("f", cultureInfo)} " +
                $"s dobou trvání {duration.ToString(@"h\:mm", cultureInfo)} byla úspěšně provedena.",
                additionalContent
            );
        }

        public async Task SendReservationCancellationByOwnerDeletingAccountEmailAsync(
            string recipientEmail, string firstName, string lastName, string reservationTitle, string organization)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Rezervace zrušena smazáním účtu",
                "Rezervace zrušena",
                "#d9534f",
                "Rezervace zrušena",
                $"tímto Vás informujeme, že rezervace <strong>\"{reservationTitle}\" z {organization} byla zrušena v důsledku smazání účtu vlastníka."
            );
        }

        public async Task SendReservationCancellationByOwnerEmailAsync(
            string recipientEmail, string firstName, string lastName,
            string reservationTitle, DateTime reservationDate, TimeZoneInfo timeZoneInfo, CultureInfo cultureInfo)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Rezervace zrušena vlastníkem",
                "Zrušení rezervace vlastníkem",
                "#d9534f",
                "Rezervace zrušena",
                $"tímto Vás informujeme, že rezervace <strong>\"{reservationTitle}\"</strong> plánovaná na {reservationDate.ConvertToTimeZone(timeZoneInfo).ToString("f", cultureInfo)} byla zrušena vlastníkem."
            );
        }

        public async Task SendDeleteAccountEmailAsync(string recipientEmail, string firstName, string lastName, string identifier)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Účet byl smazán",
                "Účet smazán",
                "#6c757d",
                "Účet smazán",
                $"tímto potvrzujeme, že Váš účet s identifikátorem {identifier} byl úspěšně smazán z naší databáze.\n" +
                "Pokud jste tuto akci neprovedli, prosím kontaktujte náš tým."
            );
        }

        private async Task SendEmailAsync(
            string recipientEmail,
            string firstName,
            string lastName,
            string subject,
            string title,
            string headerColor,
            string headerText,
            string mainText,
            string? additionalContent = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(SenderName, SenderEmail));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = subject;

            string htmlContent = GenerateEmailTemplate(
                $"{firstName} {lastName}",
                title,
                headerColor,
                headerText,
                mainText,
                additionalContent
            );

            message.Body = new TextPart("html") { Text = htmlContent };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string GenerateEmailTemplate(
            string recipientName,
            string title,
            string headerColor,
            string headerText,
            string mainText,
            string? additionalContent = null)
        {
            return $@"
            <html>
              <head>
                <meta charset=""UTF-8"">
                <title>{title}</title>
              </head>
              <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
                <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
                  <h2 style=""color: {headerColor}; text-align: center;"">{headerText}</h2>
                  <p>Dobrý den {recipientName},</p>
                  <p>{mainText}</p>
                  {additionalContent ?? ""}
                  <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
                </div>
              </body>
            </html>";
        }

        private async Task<SmtpClient> InitSmtpClient()
        {
            var client = new SmtpClient();
            await client.ConnectAsync("smtp.websupport.cz", 465, SecureSocketOptions.SslOnConnect);
            string smtpPassword = _configuration["SmtpPassword"] 
                ?? throw new InvalidOperationException("Chybí proměnná prostředí SmtpPassword");
            await client.AuthenticateAsync(SenderEmail, smtpPassword);
            return client;
        }
    }
}