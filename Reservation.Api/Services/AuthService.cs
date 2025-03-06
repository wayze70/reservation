using System.Net;
using Microsoft.EntityFrameworkCore;
using Reservation.Api.CustomException;
using Reservation.Api.Dtos;
using Reservation.Api.JWT;
using Reservation.Api.Models;
using ReservationApi;

namespace Reservation.Api.Services;

public class AuthService : IAuthService
{
    private readonly DataContext _dbContext;
    private readonly JwtTokenHelper _jwtTokenHelper;

    public AuthService(DataContext dbContext, JwtTokenHelper jwtTokenHelper)
    {
        _dbContext = dbContext;
        _jwtTokenHelper = jwtTokenHelper;
    }

    public AuthResponse Login(string email, string password)
    {
        var user = _dbContext.Owners.FirstOrDefaultAsync(user => user.Email == email && user.PasswordHash ==
            password).Result;

        if (user is null)
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Invalid email or password");
        }
        
        string accessToken = _jwtTokenHelper.GenerateAccessToken(user);
        string refreshToken = _jwtTokenHelper.GenerateRefreshToken(user);
        
        user.RefreshToken = refreshToken;
        _dbContext.SaveChanges();

        return new AuthResponse() { AccessToken = accessToken, RefreshToken = refreshToken };
    }

    public AuthResponse Register(string firstName, string lastName, string email, string password)
    {
        if (_dbContext.Owners.Any(user => user.Email == email))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Email already exists");
        }

        var createdUser = _dbContext.Owners.Add(new Owner
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = password
        }).Entity;

        _dbContext.SaveChanges();
        return Login(createdUser.Email, password);
    }

    public async Task<string> RefreshAsync(string refreshToken)
    {
        // Token validation
        var tokenClaims = _jwtTokenHelper.ValidateToken(refreshToken) 
                          ?? throw new CustomHttpException(HttpStatusCode.Unauthorized, "Invalid token");

        // Find user by refresh token
        var user = await _dbContext.Owners.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
                   ?? throw new CustomHttpException(HttpStatusCode.BadRequest, "User not found");

        return _jwtTokenHelper.GenerateAccessToken(user);
    }
}