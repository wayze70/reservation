using System.Globalization;

namespace Reservation.Api.Services;

public interface IEmailService
{
    public Task SendRegistrationSuccessEmailAsync(string email, string firstName, string lastName);
    public Task SendReservationCancellationByUserEmailAsync(string recipientEmail, string firstName, string lastName, string 
        reservationTitle, DateTime reservationDate, CultureInfo cultureInfo);
    public Task SendReservationConfirmationEmailWithUnsubscribeLinkAsync(string recipientEmail, string firstName, 
        string lastName, string reservationTitle, DateTime reservationDate, TimeSpan duration, int reservationId, string 
            unsubscribeLink, CultureInfo cultureInfo);
    public Task SendReservationCancellationByOwnerDeletingAccountEmailAsync(string recipientEmail, string firstName, string lastName, 
        string reservationTitle, string organization);

    public Task SendReservationCancellationByOwnerEmailAsync(string recipientEmail, string firstName, string lastName,
        string reservationTitle, DateTime reservationDate, TimeZoneInfo timeZoneInfo, CultureInfo cultureInfo);
    public Task SendDeleteAccountEmailAsync(string recipientEmail, string firstName, string lastName, string path);
}