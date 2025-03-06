using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Reservation.Api.Models;

namespace Reservation.Api.JWT;

public class JwtTokenHelper
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenHelper(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];
    }

    public string GenerateAccessToken(Owner owner)
    {
        IEnumerable<Claim> claims =
        [
            new Claim(ReservationClaimNames.Sub, owner.Id.ToString()),
            new Claim(ReservationClaimNames.Email, owner.Email),
            new Claim(ReservationClaimNames.GivenName, owner.FirstName),
            new Claim(ReservationClaimNames.FamilyName, owner.LastName)
        ];
        
        return GenerateToken(claims, TimeSpan.FromMinutes(20)); // 20 minut
    }

    public string GenerateRefreshToken(Owner owner)
    {
        IEnumerable<Claim> claims =
        [
            new Claim(ReservationClaimNames.Sub, owner.Id.ToString()),
            new Claim(ReservationClaimNames.Email, owner.Email),
            new Claim(ReservationClaimNames.Custom.GeneratedNumber, new Random().Next(0, 999).ToString())
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
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.UTF8.GetBytes(_key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            // Token validation
            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Kontrola algoritmu tokenu
            if (validatedToken is JwtSecurityToken jwtToken &&
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }

            return claimsPrincipal;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public static IEnumerable<Claim> GetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        return securityToken?.Claims;
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
}