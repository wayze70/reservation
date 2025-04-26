using System.Net;
using Reservation.Api.CustomException;
using Reservation.Shared.Authorization;

namespace Reservation.Api.JWT;

public static class Utils
{
    public static int GetUserIdFromBearerToken(string authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Authorization header je prázdný");
        }

        string bearerToken = JwtTokenHelper.GetBearerToken(authorization);

        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Bearer token nenalezen");
        }

        var claims = JwtTokenHelper.GetClaims(bearerToken);
        string? userIdClaim = JwtTokenHelper.GetClaimValue(claims, ReservationClaimNames.Sub);

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Uživatelské ID nenalezeno");
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Uživatelské ID není platné číslo");
        }

        return userId;
    }

    public static int GetAccountIdFromBearerToken(string authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Authorization header je prázdný");
        }

        string bearerToken = JwtTokenHelper.GetBearerToken(authorization);

        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Bearer token nenalezen");
        }

        var claims = JwtTokenHelper.GetClaims(bearerToken);
        string? userIdClaim = JwtTokenHelper.GetClaimValue(claims, ReservationClaimNames.Custom.AccountId);

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Učet ID nenalezeno");
        }

        if (!int.TryParse(userIdClaim, out int accountId))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Účet ID není platné číslo");
        }

        return accountId;
    }
}