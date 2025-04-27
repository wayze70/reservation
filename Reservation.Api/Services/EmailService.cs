using System.Globalization;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Reservation.Shared.Authorization;
using Reservation.Shared.Common;

namespace Reservation.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _senderEmail;
        private const string SenderName = "Rezervario";

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _senderEmail = configuration["EmailSettings:SenderEmail"] 
                           ?? throw new InvalidOperationException("Chybí konfigurace EmailSettings:SenderEmail");
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
            string additionalContent = $@"<p>Pokud se chcete od této události odhlásit, klikněte na následující odkaz:</p>
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
                $"tímto Vás informujeme, že rezervace <strong>\"{reservationTitle}\"</strong> plánovaná na {reservationDate.ConvertToTimeZone(timeZoneInfo).ToString("f", cultureInfo)} ({timeZoneInfo.DisplayName}) byla zrušena vlastníkem."
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
                $"tímto potvrzujeme, že Váš účet s identifikátorem \"{identifier}\" byl úspěšně smazán z naší databáze.\n" +
                "Pokud jste tuto akci neprovedli, prosím kontaktujte náš tým."
            );
        }
        
        public async Task SendNewEmployeeWelcomeEmailAsync(
            string recipientEmail, 
            string firstName, 
            string lastName, 
            string password,
            string identifier,
            string organizationName,
            Role role)
        {
            string organizationInHtml = !string.IsNullOrEmpty(organizationName) ? organizationName : "rezervačního systému";
            string identifierInHtml = !string.IsNullOrEmpty(organizationName) ? organizationName : "s identifikátorem " + $"\"{identifier}\"";
            string roleName = role.ToString();
            string additionalContent = $@"
        <div style=""background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin: 15px 0;"">
            <p style=""margin: 5px 0;""><strong>Přihlašovací údaje:</strong></p>
            <p style=""margin: 5px 0;"">Identifikátor: {identifierInHtml}</p>
            <p style=""margin: 5px 0;"">Email: {recipientEmail}</p>
            <p style=""margin: 5px 0;"">Heslo: {password}</p>
            <p style=""margin: 5px 0;"">Heslo si po přihlášení změňte v nastavení.</p>
        </div>
        <p>Vaše role je: <strong>{roleName}</strong></p>
        <p style=""text-align: center;"">
            <a href=""https://www.rezervario.cz/prihlaseni"" 
               style=""display: inline-block; padding: 10px 20px; color: #fff; background-color: #007bff; 
               text-decoration: none; border-radius: 4px;"">
               Přihlásit se
            </a>
        </p>
        <p style=""color: #666; font-size: 0.9em;"">
            Z bezpečnostních důvodů doporučujeme při prvním přihlášení změnit heslo.
        </p>";

            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                $"Vítejte v týmu {organizationInHtml}",
                "Nový účet zaměstnance",
                "#007bff",
                $"Vítejte v týmu {organizationInHtml}",
                $"byl Vám vytvořen účet v rezervačním systému organizace {identifierInHtml}. " +
                "Níže najdete přihlašovací údaje do aplikace.",
                additionalContent
            );
        }
        
        public async Task SendEmployeeDeletionEmailAsync(
            string recipientEmail, 
            string firstName, 
            string lastName, 
            string organizationName,
            string identifier)
        {
            string organizationInHtml = string.IsNullOrEmpty(organizationName) 
                ? "s identifikátorem " + $"\"{identifier}\""
                : organizationName;

            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                $"Váš účet v organizaci {organizationInHtml} byl smazán",
                "Smazání účtu zaměstnance",
                "#d9534f",
                "Váš účet byl smazán",
                $@"tímto Vás informujeme, že Váš zaměstnanecký účet v organizaci {organizationInHtml} byl smazán. 
            Ztratili jste tak přístup do rezervačního systému této organizace.",
                @"<p style=""color: #666; font-size: 0.9em; margin-top: 20px;"">
            Pokud si myslíte, že došlo k chybě, kontaktujte prosím administrátora organizace.
        </p>"
            );
        }
        
        public async Task SendCustomerRemovedFromReservationEmailAsync(
            string recipientEmail,
            string firstName,
            string lastName,
            string reservationTitle,
            DateTime reservationDate,
            TimeSpan duration,
            TimeZoneInfo timeZoneInfo,
            CultureInfo cultureInfo,
            string organization,
            string identifier)
        {
            
            string organizationInHtml = string.IsNullOrEmpty(organization) 
                ? "s identifikátorem " + $"\"{identifier}\""
                : organization;
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Odstranění z rezervace",
                "Odstranění z rezervace",
                "#d9534f",
                "Byli jste odstraněni z rezervace",
                $@"tímto Vás informujeme, že jste byli odstraněni z rezervace <strong>{reservationTitle}</strong> 
               v organizaci {organizationInHtml}, která byla plánována na {reservationDate.ConvertToTimeZone(timeZoneInfo).ToString("f", cultureInfo)} ({timeZoneInfo.DisplayName})
               s dobou trvání {duration.ToString(@"h\:mm", cultureInfo)}.",
                @"<p style=""color: #666; font-size: 0.9em; margin-top: 20px;"">
                Pokud si myslíte, že došlo k chybě, kontaktujte prosím organizátora rezervace.
            </p>"
            );
        }
        
        public async Task SendReservationUpdateEmailAsync(
            string recipientEmail,
            string firstName,
            string lastName,
            string reservationTitle,
            DateTime reservationDate,
            TimeSpan duration,
            TimeZoneInfo timeZoneInfo,
            CultureInfo cultureInfo)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Změna v rezervaci",
                "Aktualizace rezervace",
                "#ffc107",
                "Rezervace byla upravena",
                $"tímto Vás informujeme, že došlo ke změně v rezervaci <strong>{reservationTitle}</strong>. " +
                $"Nový termín je {reservationDate.ConvertToTimeZone(timeZoneInfo).ToString("f", cultureInfo)} ({timeZoneInfo.DisplayName}) " +
                $"s dobou trvání {duration.ToString(@"h\:mm", cultureInfo)}."
            );
        }
        
        public async Task SendReservationReminderEmailAsync(
            string recipientEmail,
            string firstName,
            string lastName,
            string reservationTitle,
            DateTime reservationDate,
            TimeSpan duration,
            TimeZoneInfo timeZoneInfo,
            CultureInfo cultureInfo)
        {
            await SendEmailAsync(
                recipientEmail,
                firstName,
                lastName,
                "Připomenutí události",
                "Připomenutí události",
                "#17a2b8",
                "Připomenutí nadcházející události",
                $"tímto Vám připomínáme, že zítra v {reservationDate.ConvertToTimeZone(timeZoneInfo).ToString("t", cultureInfo)} ({timeZoneInfo.DisplayName}) " +
                $"se koná událost <strong>{reservationTitle}</strong> " +
                $"s dobou trvání {duration.ToString(@"h\:mm", cultureInfo)}."
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
            message.From.Add(new MailboxAddress(SenderName, _senderEmail));
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
            string smtpPassword = _configuration["EmailSettings:SmtpPassword"] 
                ?? throw new InvalidOperationException("Chybí proměnná prostředí SmtpPassword");
            await client.AuthenticateAsync(_senderEmail, smtpPassword);
            return client;
        }
    }
}