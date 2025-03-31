namespace Reservation.Shared.Common;

public static class Utils
{
    public static bool IsPathValidate(string value)
    {
        var regex = new System.Text.RegularExpressions.Regex("^[A-Za-z0-9\\-_~]+$");
        return regex.IsMatch(value);
    }
}