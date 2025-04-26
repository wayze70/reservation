using System.Collections.ObjectModel;
using Reservation.Shared.Authorization;

namespace Reservation.Web.Client.CustomExtensions;

public static class Constants
{
    public const string ApiBaseAddress = "https://www.api.rezervario.cz/";
    public const string AppName = "Rezervario";

    public class Routes
    {
        public const string Home = "/";
        public const string Login = "/prihlaseni";
        public const string Register = "/registrace";
        public static string Reservation(string path) => $"/rezervace/{path}";

        public static string ReservationDetail(string path, int reservationId) =>
            Reservation(path) + $"/{reservationId}";

        public static string ReservationSignUp(string path, int reservationId) =>
            ReservationDetail(path, reservationId) + "/prihlaseni";

        public class AccountRoute
        {
            public const string Account = "/ucet";
            public const string AccountReservation = "/ucet/udalost";
            public const string ReservationNew = "/ucet/nova-udalost";
            public static string AccountReservationDetail(int detail) => $"/ucet/udalost/{detail}";
        }
    }

    public const string AccessToken = "accessToken";
    public const string RefreshToken = "refreshToken";
    public const string Bearer = "Bearer";
    public static readonly ReadOnlyCollection<TimeZoneInfo> TimeZones = TimeZoneInfo.GetSystemTimeZones();

    public record RoleInfo(string Name, string DisplayName, string Description)
    {
        private static readonly RoleInfo[] Roles =
        [
            new(nameof(Role.Employee), "Zaměstnanec", "Může pouze číst události"),
            new(nameof(Role.Reservationist), "Správce rezervací",
                "Může vytvářet, upravovat a mazat události i rezervace"),
            new(nameof(Role.Admin), "Administrátor", "Má přístup do celého systému"),
        ];

        public static IReadOnlyDictionary<Role, RoleInfo> GetAll() =>
            Roles.ToDictionary(
                x => Enum.Parse<Role>(x.Name),
                x => x);
    }
}