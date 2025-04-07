using System.Collections.ObjectModel;

namespace Reservation.Web.Client.CustomExtensions;

public static class Constants
{
    public const string ApiBaseAddress = "https://reservation-5wx7.onrender.com";
    
    public class Routes
    {
        public const string Home = "/";
        public const string Login = "/prihlaseni";
        public const string Register = "/registrace";
        public static string Reservation(string path) => $"/rezervace/{path}";
        public static string ReservationDetail(string path, int reservationId) => Reservation(path) + $"/{reservationId}";
        public static string ReservationSignUp(string path, int reservationId) => ReservationDetail(path, reservationId) + "/prihlaseni";
        
        public class AccountRoute
        {
            public const string Account = "/ucet";
            public const string AccountReservation = "/ucet/rezervace";
            public const string ReservarionNew = "/ucet/nova-rezervace";
            public static string AccountReservationDetail(int detail) => $"/ucet/rezervace/{detail}";
        }
    }

    public const string AccessToken = "accessToken";
    public const string RefreshToken = "refreshToken";
    public const string Bearer = "Bearer";

    public static readonly ReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();
}