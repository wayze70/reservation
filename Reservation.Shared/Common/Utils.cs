using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Reservation.Shared.Common;

public static partial class Utils
{
    [GeneratedRegex("^[A-Za-z0-9\\-_~]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
    
    public const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$";
    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex MyRegexEmail();
    
    public static bool IsPathValidate(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && MyRegex().IsMatch(value);
    }
    
    public static bool TryProcessEmail(string input, out string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                email = input;
                return false;
            }

            string processedEmail = input.Trim().ToLowerInvariant();
            bool isValid = new EmailAddressAttribute().IsValid(processedEmail);
        
            if (!isValid || !MyRegexEmail().IsMatch(processedEmail))
            {
                email = input;
                return false;
            }

            email = processedEmail;
            return true;
        }
        catch
        {
            email = input;
            return false;
        }
    }

    public static bool IsPasswordLongEnough(string password)
    {
        return password.Length is >= 6 and <= 200;
    }
}