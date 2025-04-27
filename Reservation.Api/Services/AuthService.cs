using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.JWT;
using Reservation.Api.Models;
using Reservation.Shared.Authorization;
using Reservation.Shared.Dtos;
using ReservationClaimNames = Reservation.Shared.Authorization.ReservationClaimNames;
using Utils = Reservation.Shared.Common.Utils;

namespace Reservation.Api.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _dbContext;
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;

    public AuthService(DataContext dbContext, JwtTokenHelper jwtTokenHelper, IEmailService emailService,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _jwtTokenHelper = jwtTokenHelper;
        _passwordHasher = new PasswordHasher<User>();
        _emailService = emailService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, string identifier, string deviceName)
    {
        var account = await _dbContext.Accounts
                          .Include(a => a.Users)
                          .FirstOrDefaultAsync(a => a.Path == identifier)
                      ?? throw new CustomHttpException(HttpStatusCode.NotFound,
                          "Účet s tímto identifikátorem nebyl nalezen");

        var user = account.Users.FirstOrDefault(u => u.Email == email);
        if (user is null)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní email nebo heslo");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verificationResult != PasswordVerificationResult.Success &&
            verificationResult != PasswordVerificationResult.SuccessRehashNeeded)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní email nebo heslo");
        }

        deviceName = deviceName.ToLower();

        string accessToken = _jwtTokenHelper.GenerateAccessToken(user, account.Id);
        string refreshToken = _jwtTokenHelper.GenerateRefreshToken(user, account.Id, deviceName);

        var device = new Device
        {
            DeviceName = deviceName,
            RefreshToken = refreshToken,
            UserId = user.Id,
            User = user
        };

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync();

        return new AuthResponse { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public async Task<AuthResponse> RegisterAsync(string firstName, string lastName,
        string identifier, string email, string password, string deviceName)
    {
        identifier = identifier.Trim().ToLowerInvariant();
        email = email.Trim().ToLowerInvariant();

        if (!Utils.IsPasswordLongEnough(password))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Heslo musí mít alespoň 6 znaků");

        if (!Utils.TryProcessEmail(email, out email))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Email nemá validní formát");

        if (!Utils.IsPathValidate(identifier))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Identifikátor účtu není validní");
        }

        var existingCheck = await _dbContext.Accounts
            .Include(a => a.Users)
            .FirstOrDefaultAsync(a => a.Path == identifier);

        if (existingCheck != null)
            throw new CustomHttpException(HttpStatusCode.Conflict, "Účet s tímto identifikátorem již existuje");

        if (existingCheck?.Users.Any(o => o.Email == email) == true)
            throw new CustomHttpException(HttpStatusCode.Conflict,
                "Uživatel s tímto emailem již existuje u daného účtu");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var account = new Account
            {
                Path = identifier
            };

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            var owner = new User
            {
                AccountId = account.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = Role.Admin,
                PasswordHash = _passwordHasher.HashPassword(new User(), password)
            };

            _dbContext.Users.Add(owner);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _emailService.SendRegistrationSuccessEmailAsync(owner.Email, owner.FirstName, owner.LastName);
            return await LoginAsync(owner.Email, password, identifier, deviceName);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> RefreshAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");

        if (_jwtTokenHelper.ValidateToken(refreshToken) is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní token");

        int ownerId = GetOwnerId(refreshToken);
        
        var owner = await _dbContext.Users
            .Include(o => o.Account)
            .Include(o => o.Devices)
            .FirstOrDefaultAsync(o =>
                o.Id == ownerId &&
                o.Devices.Any(d => d.RefreshToken == refreshToken));


        if (owner is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatný refresh token nebo vlastník");

        return _jwtTokenHelper.GenerateAccessToken(owner, owner.Account.Id);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");

        if (_jwtTokenHelper.ValidateTokenOrigin(refreshToken) is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Nevalidní token");

        int ownerId = GetOwnerId(refreshToken);

        var device = await _dbContext.Devices
            .Include(d => d.User)
            .FirstOrDefaultAsync(d =>
                d.RefreshToken == refreshToken &&
                d.UserId == ownerId);

        if (device is null)
            throw new CustomHttpException(HttpStatusCode.BadRequest,
                "Zařízení nenalezeno nebo nepatří danému uživateli");

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

        var user = await _dbContext.Users
            .Include(u => u.Devices)
            .FirstOrDefaultAsync(u => u.Id == ownerId);

        if (user == null)
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatný uživatel");

        if (!user.Devices.Any(d => d.RefreshToken == refreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Neplatné zařízení");

        _dbContext.Devices.RemoveRange(user.Devices);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static int GetOwnerId(string refreshToken)
    {
        return int.Parse(JwtTokenHelper
            .GetClaimValue(JwtTokenHelper.GetClaims(refreshToken), ReservationClaimNames.Sub) ?? throw new
            CustomHttpException(HttpStatusCode.BadRequest, "User id в refresh token není platný"));
    }
}