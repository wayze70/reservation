using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.CustomException;
using Reservation.Api.Services;
using Reservation.Shared.Authorization;
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
        string userAgent = Request.Headers.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = UnknownDevice;
        }

        return Ok(await _authService.LoginAsync(request.Email, request.Password, request.Identifier, userAgent));
    }


    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegistrationRequest request)
    {
        string userAgent = Request.Headers.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = UnknownDevice;
        }

        return Ok(await _authService.RegisterAsync(request.FirstName, request.LastName, request.Identifier,
            request.Email, request.Password, userAgent));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<string>> Refresh([FromBody] RefreshTokenRequest request)
    {
        return Ok(await _authService.RefreshAsync(request.RefreshToken));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<bool>> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");
        }

        return Ok(await _authService.LogoutAsync(request.RefreshToken));
    }

    [HttpPost("logout-all")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<bool>> LogoutAllDevices([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new CustomHttpException(HttpStatusCode.BadRequest, "Refresh token je prázdný");
        }

        return Ok(await _authService.LogoutAllDevicesAsync(request.RefreshToken));
    }
}