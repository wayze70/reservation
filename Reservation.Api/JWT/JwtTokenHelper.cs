using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Reservation.Api.Models;
using Reservation.Shared.Authorization;

namespace Reservation.Api.JWT;

public class JwtTokenHelper
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenHelper(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]
               ?? throw new InvalidOperationException("Chybí konfigurace 'Jwt:Key'.");
        _issuer = configuration["Jwt:Issuer"]
                  ?? throw new InvalidOperationException("Chybí konfigurace 'Jwt:Issuer'.");
        _audience = configuration["Jwt:Audience"]
                    ?? throw new InvalidOperationException("Chybí konfigurace 'Jwt:Audience'.");
    }

    public string GenerateAccessToken(User user, int accountId)
    {
        IEnumerable<Claim> claims =
        [
            new Claim(ReservationClaimNames.Sub, user.Id.ToString()),
            new Claim(ReservationClaimNames.Custom.AccountId, accountId.ToString()),
            new Claim(ReservationClaimNames.Email, user.Email),
            new Claim(ReservationClaimNames.GivenName, user.FirstName),
            new Claim(ReservationClaimNames.FamilyName, user.LastName),
            new Claim(ReservationClaimNames.Custom.Role, user.Role.ToString()),
        ];

        return GenerateToken(claims, TimeSpan.FromMinutes(10)); // 10 minut
    }

    public string GenerateRefreshToken(User user, int accountId, string deviceName)
    {
        IEnumerable<Claim> claims =
        [
            new Claim(ReservationClaimNames.Sub, user.Id.ToString()),
            new Claim(ReservationClaimNames.Custom.AccountId, accountId.ToString()),
            new Claim(ReservationClaimNames.Email, user.Email),
            new Claim(ReservationClaimNames.Custom.GeneratedNumber, new Random().Next(0, 999).ToString()),
            new Claim(ReservationClaimNames.Custom.DeviceName, deviceName),
            new Claim(ReservationClaimNames.Custom.Role, user.Role.ToString()),
        ];

        return GenerateToken(claims, TimeSpan.FromDays(180)); // 6 měsíců
    }

    private string GenerateToken(IEnumerable<Claim> claims, TimeSpan validFor)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(validFor),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
        => ValidateTokenInternal(token, validateLifetime: true);

    public ClaimsPrincipal? ValidateTokenOrigin(string token)
        => ValidateTokenInternal(token, validateLifetime: false);

    public static IEnumerable<Claim> GetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return securityToken?.Claims ?? [];
    }

    public static string? GetClaimValue(IEnumerable<Claim>? claims, string claimType)
    {
        return claims?.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    public static string GetBearerToken(string authorization)
    {
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer "))
        {
            return string.Empty;
        }

        return authorization["Bearer ".Length..].Trim();
    }
    
    private TokenValidationParameters CreateBaseValidationParameters(bool validateLifetime = true)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = validateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            ClockSkew = TimeSpan.Zero
        };
    }

    private ClaimsPrincipal? ValidateTokenInternal(string token, bool validateLifetime)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = CreateBaseValidationParameters(validateLifetime);

            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Nevalidní algoritmus tokenu");
            }

            return claimsPrincipal;
        }
        catch
        {
            return null;
        }
    }
}