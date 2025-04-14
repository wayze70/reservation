using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.JWT;
using Reservation.Api.Services;
using Reservation.Shared.Common;
using Reservation.Shared.Dtos;

namespace Reservation.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private const string UnknownDevice = "Unknown device";

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = UnknownDevice;
        }

        // Předáme také DeviceName do metody LoginAsync
        var authResponse = await _authService.LoginAsync(request.Email, request.Password, request.Identifier, userAgent);
        return Ok(authResponse);
    }


    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegistrationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        string userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = UnknownDevice;
        }

        var authResponse = await _authService.RegisterAsync(request.FirstName, request.LastName, request.Organization, request.Identifier, request.Email,
            request.Password, userAgent);

        return Ok(authResponse);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<string>> Refresh([FromBody] RefreshTokenRequest request)
    {
        string newAccessToken = await _authService.RefreshAsync(request.RefreshToken);
        return Ok(newAccessToken);
    }
    
    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");

        bool response = await _authService.LogoutAsync(request.RefreshToken);
        return Ok(response);
    }
    
    [HttpPost("logout-all")]
    public async Task<ActionResult<bool>> LogoutAllDevices([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");
        
        bool response = await _authService.LogoutAllDevicesAsync(request.RefreshToken);
        return Ok(response);
    }
}