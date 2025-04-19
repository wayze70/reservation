using System.Text.RegularExpressions;

namespace Reservation.Shared.Common;

public static partial class Utils
{
    [GeneratedRegex("^[A-Za-z0-9\\-_~]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
    private static readonly Regex PathRegex = MyRegex();
    
    public static bool IsPathValidate(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && PathRegex.IsMatch(value);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var mail = new System.Net.Mail.MailAddress(email);
            return mail.Address == email;
        }
        catch
        {
            return false;
        }
    }
}