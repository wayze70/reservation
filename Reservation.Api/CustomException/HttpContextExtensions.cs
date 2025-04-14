using System.Net;
using Microsoft.VisualBasic.CompilerServices;

namespace Reservation.Api.CustomException;

public static class HttpContextExtensions
{
    public static string GetJwtBearerToken(this HttpContext httpContext)
    {
        string token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            throw new CustomHttpException(HttpStatusCode.Unauthorized, "Token not found");
        }
        return token;
    }

    public static int GetAccountId(this HttpContext httpContext)
    {
        return JWT.Utils.GetAccountIdFromBearerToken(httpContext.Request.Headers.Authorization.ToString());
    }

    public static int GetUserId(this HttpContext httpContext)
    {
        return JWT.Utils.GetUserIdFromBearerToken(httpContext.Request.Headers.Authorization.ToString());
    }
}