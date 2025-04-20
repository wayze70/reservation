namespace Reservation.Shared.Common;

public static class DateTimeExtensions
{
    public static DateTime ConvertToTimeZone(this DateTime dateTime, string timeZoneId)
    {
        return dateTime.AddMinutes(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).GetUtcOffset(dateTime).TotalMinutes);
    }
    
    public static DateTime ConvertToTimeZone(this DateTime dateTime, TimeZoneInfo timeZone)
    {
        return dateTime.AddMinutes(timeZone.GetUtcOffset(dateTime).TotalMinutes);
    }
}