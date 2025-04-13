using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Reservation.Api.CustomException;
using Reservation.Api.JWT;
using Reservation.Api.Models;
using Reservation.Shared.Dtos;
using Utils = Reservation.Shared.Common.Utils;

namespace Reservation.Api.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _dbContext;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IPasswordHasher<Owner> _passwordHasher;
    private readonly IEmailService _emailService;

    public AuthService(DataContext dbContext, JwtTokenHelper jwtTokenHelper, IEmailService emailService)
    {
        _dbContext = dbContext;
        _jwtTokenHelper = jwtTokenHelper;
        _passwordHasher = new PasswordHasher<Owner>();
        _emailService = emailService;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, string deviceName)
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
        
        deviceName = deviceName.ToLower();
    
        string accessToken = _jwtTokenHelper.GenerateAccessToken(user);
        string refreshToken = _jwtTokenHelper.GenerateRefreshToken(user, deviceName);
    
        var device = new Device
        {
            DeviceName = deviceName,
            RefreshToken = refreshToken,
            OwnerId = user.Id,
            Owner = user
        };

        // Přidáme nový záznam zařízení do databáze
        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<AuthResponse> RegisterAsync(string firstName, string lastName, string organization, string 
            email, string password, string deviceName)
    {
        if (await _dbContext.Owners.AnyAsync(u => u.Email == email))
        {
            throw new CustomHttpException(HttpStatusCode.Conflict, "Uživatel s tímto emailem již existuje");
        }
        
        if (password.Length < 6)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Heslo musí mít alespoň 6 znaků");
        }

        if (!Utils.IsValidEmail(email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Email nemá validní formát");
        }

        var newOwner = new Owner
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Organization = organization
        };

        // Hashování hesla
        newOwner.PasswordHash = _passwordHasher.HashPassword(newOwner, password);

        _dbContext.Owners.Add(newOwner);
        await _dbContext.SaveChangesAsync();
        await _emailService.SendRegistrationSuccessEmailAsync(newOwner.Email, newOwner.FirstName, newOwner.LastName);

        // Pro zjednodušení voláme login, který vygeneruje tokeny
        return await LoginAsync(newOwner.Email, password, deviceName);
    }

    public async Task<string> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");

        if (_jwtTokenHelper.ValidateToken(refreshToken) is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní token");
        
        var owner = await _dbContext.Owners
            .Include(o => o.Devices)
            .FirstOrDefaultAsync(o => o.Id == GetOwnerId(refreshToken) && o.Devices.Any(d => d.RefreshToken == refreshToken));

        if (owner is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatný refresh token nebo vlastník");

        return _jwtTokenHelper.GenerateAccessToken(owner);
    }
    
    public async Task<bool> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");
        
        if (_jwtTokenHelper.ValidateTokenOrigin(refreshToken) is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní token");

        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken && d.OwnerId == GetOwnerId(refreshToken));
        
        if (device is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Zařízení nenalezeno");
    
        _dbContext.Devices.Remove(device);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task CleanupInvalidRefreshTokensAsync()
    {
        var devices = await _dbContext.Devices.ToListAsync();
        var invalidDevices = devices.Where(d => _jwtTokenHelper.ValidateToken(d.RefreshToken) is null);
    
        _dbContext.Devices.RemoveRange(invalidDevices);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> LogoutAllDevicesAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");

        if (_jwtTokenHelper.ValidateTokenOrigin(refreshToken) is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní token");

        int ownerId = GetOwnerId(refreshToken);
    
        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken);
    
        if (device is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Zařízení nenalezeno");

        // Pak odstraníme všechna zařízení vlastníka
        var devices = await _dbContext.Devices
            .Where(d => d.OwnerId == ownerId)
            .ToListAsync();

        _dbContext.Devices.RemoveRange(devices);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    private static int GetOwnerId(string refreshToken)
    {
        return int.Parse(JwtTokenHelper
            .GetClaimValue(JwtTokenHelper.GetClaims(refreshToken), ReservationClaimNames.Sub) ?? throw new
            CustomHttpException(HttpStatusCode.BadRequest, "User id v refresh token není platný"));
    }
}
