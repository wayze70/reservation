using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Utils = Reservation.Api.JWT.Utils;

namespace Reservation.Api.CustomException;

public static class HttpContextExtensions
{
    private static readonly CultureInfo[] CultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
        .Where(n => !Equals(n.Parent, CultureInfo.InvariantCulture)).ToArray();
    
    public static string GetJwtBearerToken(this HttpContext httpContext)
    {
        string token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            throw new CustomHttpException(HttpStatusCode.Unauthorized, "Token nenalezen");
        }
        return token;
    }

    public static int GetAccountIdFromBearer(this HttpContext httpContext)
    {
        return Utils.GetAccountIdFromBearerToken(httpContext.Request.Headers.Authorization.ToString());
    }

    public static int GetUserIdFromBearer(this HttpContext httpContext)
    {
        return Utils.GetUserIdFromBearerToken(httpContext.Request.Headers.Authorization.ToString());
    }
    
    public static string[]? GetUserLanguages(this HttpContext httpContext)
    {
        string languageHeader = httpContext.Request.Headers.AcceptLanguage.ToString();

        if (!string.IsNullOrEmpty(languageHeader))
        {
            return languageHeader.Split(',')
                .Select(StringWithQualityHeaderValue.Parse)
                .OrderByDescending(s => s.Quality.GetValueOrDefault(1))
                .Select(s => s.Value).ToArray();
        }

        return null;
    }

    public static CultureInfo GetUserPreferredCurrentCulture(this HttpContext httpContext)
    {
        string[]? languages = httpContext.GetUserLanguages();
        var specificCulture =  new CultureInfo(CultureUtilities.CultureConstants.Czech);

        if (languages is not { Length: > 0 }) return specificCulture;

        foreach (string lang in languages)
        {
            string expandedLang = CultureUtilities.ExpandCulture(lang);

            try
            {
                specificCulture = CultureInfo.CreateSpecificCulture(expandedLang);

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

        return specificCulture;
    }
}