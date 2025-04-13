using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Globalization;

namespace Reservation.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendRegistrationSuccessEmailAsync(string recipientEmail, string firstName, string lastName)
        {
            string recipientName = $"{firstName} {lastName}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rezervario", "info@rezervario.cz"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Vítejte v Rezervario!";

            // HTML obsah e-mailu s inline styly
            string htmlContent = @"
                <html>
                  <head>
                    <meta charset=""UTF-8"">
                    <title>Registrace úspěšná</title>
                  </head>
                  <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
                    <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
                      <h2 style=""color: #007BFF; text-align: center;"">Vítejte v Rezervario!</h2>
                      <p>Dobrý den " + recipientName + @",</p>
                      <p>Děkujeme vám za registraci v naší aplikaci. Vaše registrace proběhla úspěšně a nyní můžete využívat všechny výhody naší platformy.</p>
                      <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
                    </div>
                  </body>
                </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendReservationCancellationByUserEmailAsync(string recipientEmail, string firstName, string
            lastName, string reservationTitle, DateTime reservationDate, CultureInfo cultureInfo)
        {
            string recipientName = $"{firstName} {lastName}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rezervario", "info@rezervario.cz"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Rezervace byla zrušena";

            // HTML obsah pro oznámení o zrušení rezervace
            string htmlContent = $@"
                <html>
                  <head>
                    <meta charset=""UTF-8"">
                    <title>Zrušení rezervace</title>
                  </head>
                  <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
                    <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
                      <h2 style=""color: #d9534f; text-align: center;"">Rezervace byla zrušena</h2>
                      <p>Dobrý den {recipientName},</p>
                      <p>tímto Vás informujeme, že rezervace <strong>“{reservationTitle}”</strong> plánovaná na {reservationDate.ToString("f", cultureInfo)} byla zrušena.</p>
                      <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
                    </div>
                  </body>
                </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
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
            string recipientName = $"{firstName} {lastName}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rezervario", "info@rezervario.cz"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Potvrzení rezervace";

            // HTML obsah emailu s odhlašovacím odkazem
            string htmlContent = $@"
                <html>
                  <head>
                    <meta charset=""UTF-8"">
                    <title>Potvrzení rezervace</title>
                  </head>
                  <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
                    <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
                      <h2 style=""color: #28a745; text-align: center;"">Rezervace úspěšná</h2>
                      <p>Dobrý den {recipientName},</p>
                      <p>Vaše rezervace <strong>{reservationTitle}</strong> na datum {reservationDate.ToString("f",
                          cultureInfo)} s dobou trvání {duration.ToString(@"h\:mm", cultureInfo)} byla úspěšně provedena.</p>
                      <p>Pokud se chcete od této rezervace odhlásit, klikněte na následující odkaz:</p>
                      <p style=""text-align: center;"">
                          <a href=""https://www.rezervario.cz/zruseni-rezervace/{reservationId}/{unsubscribeLink}"" style=""display: inline-block; padding: 10px 20px; color: #fff; background-color: #d9534f; text-decoration: none; border-radius: 4px;"">Odhlásit rezervaci</a>
                      </p>
                      <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
                    </div>
                  </body>
                </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendReservationCancellationByOwnerDeletingAccountEmailAsync(string recipientEmail, string firstName, string lastName, 
            string reservationTitle, string organization)
        {
            string recipientName = $"{firstName} {lastName}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rezervario", "info@rezervario.cz"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Rezervace zrušena smazáním účtu";

            // HTML obsah emailu s informací, že rezervace byla zrušena kvůli smazání účtu vlastníka
            string htmlContent = $@"
    <html>
      <head>
        <meta charset=""UTF-8"">
        <title>Rezervace zrušena</title>
      </head>
      <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
        <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
          <h2 style=""color: #d9534f; text-align: center;"">Rezervace zrušena</h2>
          <p>Dobrý den {recipientName},</p>
          <p>tímto Vás informujeme, že rezervace <strong>“{reservationTitle}” z {organization} byla zrušena v důsledku smazání účtu vlastníka.</p>
          <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
        </div>
      </body>
    </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }


        public async Task SendReservationCancellationByOwnerEmailAsync(
            string recipientEmail, string firstName, string lastName,
            string reservationTitle, DateTime reservationDate, CultureInfo cultureInfo)
        {
            string recipientName = $"{firstName} {lastName}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rezervario", "info@rezervario.cz"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Rezervace zrušena vlastníkem";

            // HTML obsah emailu oznamující, že rezervaci zrušil vlastník
            string htmlContent = $@"
    <html>
      <head>
        <meta charset=""UTF-8"">
        <title>Zrušení rezervace vlastníkem</title>
      </head>
      <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
        <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
          <h2 style=""color: #d9534f; text-align: center;"">Rezervace zrušena</h2>
          <p>Dobrý den {recipientName},</p>
          <p>tímto Vás informujeme, že rezervace <strong>“{reservationTitle}”</strong> plánovaná na {reservationDate.ToString("f", cultureInfo)} byla zrušena vlastníkem.</p>
          <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
        </div>
      </body>
    </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendDeleteAccountEmailAsync(string recipientEmail, string firstName, string lastName)
        {
            string recipientName = $"{firstName} {lastName}";
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Rezervario", "info@rezervario.cz"));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = "Účet byl smazán";

            // HTML obsah emailu oznamující, že účet byl smazán
            string htmlContent = $@"
    <html>
      <head>
        <meta charset=""UTF-8"">
        <title>Účet smazán</title>
      </head>
      <body style=""font-family: Arial, sans-serif; background-color:#f9f9f9; margin:0; padding:20px;"">
        <div style=""max-width:600px; margin: auto; background: #ffffff; padding: 20px; border: 1px solid #ddd;"">
          <h2 style=""color: #6c757d; text-align: center;"">Účet smazán</h2>
          <p>Dobrý den {recipientName},</p>
          <p>tímto potvrzujeme, že Váš účet byl úspěšně smazán z naší databáze.</p>
          <p>Pokud jste tuto akci neprovedli, prosím kontaktujte náš tým.</p>
          <p>S pozdravem,<br/><strong>Tým Rezervario</strong></p>
        </div>
      </body>
    </html>";

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = await InitSmtpClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private async Task<SmtpClient> InitSmtpClient()
        {
            var client = new SmtpClient();
            await client.ConnectAsync("smtp.websupport.cz", 465, SecureSocketOptions.SslOnConnect);
            string smtpPassword = _configuration["SmtpPassword"] ?? throw new InvalidOperationException("Chybí proměnná prostředí SmtpPassword");
            await client.AuthenticateAsync("info@rezervario.cz", smtpPassword);
            return client;
        }
    }
}