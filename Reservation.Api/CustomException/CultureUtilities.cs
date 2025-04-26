namespace Reservation.Api.CustomException;

public static class CultureUtilities
{
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
    
    public static string ExpandCulture(string cultureCode)
    {
        return SupportedLanguageCultures.GetValueOrDefault(cultureCode, cultureCode);
    }
}