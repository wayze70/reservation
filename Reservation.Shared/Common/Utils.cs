using System.Globalization;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace Reservation.Shared.Common;

public static partial class Utils
{
    [GeneratedRegex("^[A-Za-z0-9\\-_~]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
    private static readonly Regex PathRegex = MyRegex();
    private static readonly CultureInfo[] CultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Where(n => !Equals(n.Parent, CultureInfo.InvariantCulture)).ToArray();
    
    public static class Languages
    {
        public const string Czech = "cs";
        public const string Slovak = "sk";
        public const string English = "en";

    }

    public static class CultureConstants
    {
        public const string Czech = "cs-CZ";
        public const string Slovak = "sk-SK";
        public const string English = "en-US";
    }
    
    public static readonly Dictionary<string, string> SupportedLanguageCultures = new Dictionary<string, string>
    {
        { Languages.Czech, CultureConstants.Czech },
        { Languages.Slovak, CultureConstants.Slovak },
        { Languages.English, CultureConstants.English },
    };
    
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

    public static string[] GetUserLanguages(string acceptedLanguageHeader)
    {
        if (!string.IsNullOrEmpty(acceptedLanguageHeader))
        {
            return acceptedLanguageHeader.Split(',')
                .Select(StringWithQualityHeaderValue.Parse)
                .OrderByDescending(s => s.Quality.GetValueOrDefault(1))
                .Select(s => s.Value).ToArray();
        }

        return null;
    }

    public static CultureInfo GetUserPreferredCurrentCulture(string[] acceptedLanguage)
    {

        if (acceptedLanguage is not { Length: > 0 }) return CultureInfo.CreateSpecificCulture(CultureConstants.Czech);

        foreach (string lang in acceptedLanguage)
        {
            string expandedLang = ExpandCulture(lang);

            try
            {
                var specificCulture = CultureInfo.CreateSpecificCulture(expandedLang);

                if (CultureInfos.Any(c => c.Name.Equals(specificCulture.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    return specificCulture;
                }
            }
            catch (CultureNotFoundException)
            {
                continue;
            }
        }
        
        return CultureInfo.CreateSpecificCulture(CultureConstants.Czech);
    }

    private static string ExpandCulture(string cultureCode)
    {
        return SupportedLanguageCultures.GetValueOrDefault(cultureCode, cultureCode);
    }
}