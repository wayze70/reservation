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
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
    {
        var tokens = _authService.Login(request.Email, request.Password);
        return Ok(tokens);
    }
    
    [HttpPost("register")]
    public ActionResult<AuthResponse> Register([FromBody] RegistrationRequest request)
    {
        return _authService.Register(request.FirstName, request.LastName, request.Email, request.Password);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<string>> Refresh([FromBody] RefreshTokenRequest request)
    {
        return await _authService.RefreshAsync(request.RefreshToken);
    }
}