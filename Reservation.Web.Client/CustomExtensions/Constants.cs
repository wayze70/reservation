namespace Reservation.Web.Client.CustomExtensions;

public static class Constants
{
    public const string ApiBaseAddress = "https://reservation-5wx7.onrender.com";
    
    public class Routes
    {
        public const string Home = "/";
        public const string Login = "/prihlaseni";
        public const string Register = "/registrace";
        public const string Reservation = "/rezervace";
        public const string Account = "/ucet";
    }

    public const string AccessToken = "accessToken";
    public const string RefreshToken = "refreshToken";
    public const string Bearer = "Bearer";

    public static readonly List<TimeSpan> TimeZones = TimeZoneInfo.GetSystemTimeZones()
        .Select(tz => tz.BaseUtcOffset).Distinct().OrderBy(offset => offset).ToList();
}