using System.Globalization;
using Reservation.Shared.Authorization;

namespace Reservation.Api.Services;

public interface IEmailService
{
    Task SendRegistrationSuccessEmailAsync(string email, string firstName, string lastName);
    Task SendReservationCancellationByUserEmailAsync(string recipientEmail, string firstName, string lastName,
        string reservationTitle, DateTime reservationDate, CultureInfo cultureInfo);
    Task SendReservationConfirmationEmailWithUnsubscribeLinkAsync(string recipientEmail, string firstName,
        string lastName, string reservationTitle, DateTime reservationDate, TimeSpan duration, int reservationId, string
            unsubscribeLink, CultureInfo cultureInfo);
    Task SendReservationCancellationByOwnerDeletingAccountEmailAsync(string recipientEmail, string firstName,
        string lastName, string reservationTitle, string organization);
    Task SendReservationCancellationByOwnerEmailAsync(string recipientEmail, string firstName, string lastName,
        string reservationTitle, DateTime reservationDate, TimeZoneInfo timeZoneInfo, CultureInfo cultureInfo);
    Task SendDeleteAccountEmailAsync(string recipientEmail, string firstName, string lastName, string path);
    Task SendNewEmployeeWelcomeEmailAsync(string recipientEmail, string firstName, string lastName,
        string password, string identifier, string organizationName, Role role);
    Task SendEmployeeDeletionEmailAsync(string recipientEmail, string firstName, string lastName,
        string organizationName, string identifier);
    Task SendCustomerRemovedFromReservationEmailAsync(string recipientEmail, string firstName, string lastName,
        string reservationTitle, DateTime reservationDate, TimeSpan duration, TimeZoneInfo timeZoneId,
        CultureInfo cultureInfo, string organization, string identifier);
}