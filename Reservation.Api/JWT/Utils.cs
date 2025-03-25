using System.Net;
using Reservation.Api.CustomException;

namespace Reservation.Api.JWT;

public static class Utils
{
    public static int GetUserIdFromAuthorizationHeader(string authorization)
    {
        string bearerToken = JwtTokenHelper.GetBearerToken(authorization);
        
        if (string.IsNullOrEmpty(bearerToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Bearer token nenalezen");
        }
        
        var claims = JwtTokenHelper.GetClaims(bearerToken);
        
        return int.Parse(JwtTokenHelper.GetClaimValue(claims, ReservationClaimNames.Sub) ?? throw new CustomHttpException(HttpStatusCode.BadRequest, "Uživatelské ID nenalezeno"));
    }
}