using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Reservation.Api.CustomException;
using Reservation.Api.JWT;
using Reservation.Api.Models;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _dbContext;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IPasswordHasher<Owner> _passwordHasher;

    public AuthService(DataContext dbContext, JwtTokenHelper jwtTokenHelper)
    {
        _dbContext = dbContext;
        _jwtTokenHelper = jwtTokenHelper;
        _passwordHasher = new PasswordHasher<Owner>();
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        // Najdeme uživatele podle emailu (heslo ověříme až dále)
        var user = await _dbContext.Owners.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní email nebo heslo");
        }
        
        // Ověříme heslo pomocí hashování
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verificationResult != PasswordVerificationResult.Success &&
            verificationResult != PasswordVerificationResult.SuccessRehashNeeded)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní email nebo heslo");
        }
        
        string accessToken = _jwtTokenHelper.GenerateAccessToken(user);
        string refreshToken = _jwtTokenHelper.GenerateRefreshToken(user);
        
        user.RefreshToken = refreshToken;
        await _dbContext.SaveChangesAsync();

        return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<AuthResponse> RegisterAsync(string firstName, string lastName, string email, string password)
    {
        if (await _dbContext.Owners.AnyAsync(u => u.Email == email))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Uživatel s tímto emailem již existuje");
        }
        
        if (password.Length < 6)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Heslo musí mít alespoň 6 znaků");
        }

        var newOwner = new Owner
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        // Hashování hesla
        newOwner.PasswordHash = _passwordHasher.HashPassword(newOwner, password);

        _dbContext.Owners.Add(newOwner);
        await _dbContext.SaveChangesAsync();

        // Pro zjednodušení voláme login, který vygeneruje tokeny
        return await LoginAsync(newOwner.Email, password);
    }

    public async Task<string> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");
        }

        var tokenClaims = _jwtTokenHelper.ValidateToken(refreshToken)
                          ?? throw new CustomHttpException(HttpStatusCode.Unauthorized, "Nevalidní token");

        var user = await _dbContext.Owners.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
                   ?? throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token nenalezen");

        return _jwtTokenHelper.GenerateAccessToken(user);
    }
}
