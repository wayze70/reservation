using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Utils = Reservation.Api.JWT.Utils;

namespace Reservation.Api.CustomException;

public static class HttpContextExtensions
{
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
}