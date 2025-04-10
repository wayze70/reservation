using System.Globalization;

namespace Reservation.Web.Client.CustomExtensions;

public static class CultureInfoExtensions
{
    public static bool Is24Hrs(this CultureInfo cultureInfo)
    {
        return cultureInfo.DateTimeFormat.ShortTimePattern.Contains('H');
    }
}