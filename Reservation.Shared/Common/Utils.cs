using System.Text.RegularExpressions;

namespace Reservation.Shared.Common;

public static partial class Utils
{
    private static readonly Regex PathRegex = MyRegex();

    public static bool IsPathValidate(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && PathRegex.IsMatch(value);
    }

    [GeneratedRegex("^[A-Za-z0-9\\-_~]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}