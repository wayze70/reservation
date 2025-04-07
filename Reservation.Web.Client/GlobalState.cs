namespace Reservation.Web.Client;

public static class GlobalState
{
    public static TimeZoneInfo CurrentTimeZone { get; set; } = TimeZoneInfo.Local;
    public static DateTime? SelectedDate { get; set; } = DateTime.Now;
    public static DateTime? SelectedDateAdmin { get; set; } = DateTime.Now;

}