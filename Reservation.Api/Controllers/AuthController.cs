using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservation.Api.Services;
using Reservation.Shared.Dtos;
using LoginRequest = Reservation.Shared.Dtos.LoginRequest;

namespace Reservation.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var authResponse = await _authService.LoginAsync(request.Email, request.Password);
        return Ok(authResponse);
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegistrationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var authResponse = await _authService.RegisterAsync(request.FirstName, request.LastName, request.Email, request.Password);
        return Ok(authResponse);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<string>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var newAccessToken = await _authService.RefreshAsync(request.RefreshToken);
        return Ok(newAccessToken);
    }
}
